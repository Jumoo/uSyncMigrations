(function () {
    'use strict';

    function migrateController($scope, $q, $timeout,
        editorService, uSyncHub,
        uSyncMigrationService, uSync8DashboardService,
        notificationsService) {

        var vm = this;
        vm.showSetup = true;
        vm.working = false;
        vm.state = 'init';
        vm.sourceValid = true;

        vm.progress = 'init';

        vm.error = '';

        vm.profile = $scope.model.profile;
        vm.options = vm.profile.options;

        vm.close = close;
        vm.migrate = migrate;

        vm.pickSource = pickSource;
        vm.pickTarget = pickTarget;
        vm.pickHandlers = pickHandlers;

        vm.$onInit = function () {
            InitHub();
            validateSource(vm.options.sourceVersion, vm.options.source);
            validate(vm.options);
            shuffleMessages(vm.messages);
        };

        //

        function migrate() {
            vm.showSetup = false; 
            vm.working = true;
            vm.state = 'busy';
            vm.progress = 'migrating'

            doMigrationMessages();

            uSyncMigrationService.migrate(vm.options)
                .then(function (result) {
                    vm.state = 'success';
                    vm.progress = 'migrated';
                    vm.results = result.data;
                    vm.working = false;
                }, function (error) {
                    vm.state = 'error';
                    vm.working = false;
                    vm.error = error.data.ExceptionMessage;
                    notificationsService.error('error', error.data.ExceptionMessage);
                })
        }

        /////

        function pickSource() {
            pickFolder(function (folder) {
                vm.options.source = folder;
                validateSource(vm.options.sourceVersion, vm.options.source);
                validate(vm.options);
            });
        }

        function validateSource(version, source) {

            uSyncMigrationService.validateSource(version, source)
                .then(function (result) {
                    vm.sourceValid = result.data.length == 0;
                    vm.sourceError = result.data;
                }, function (error) {
                    vm.error = error.data.ExceptionMessage;
                });
        }

        function validate(options) {
            uSyncMigrationService.validate(options)
                .then(function (result) {
                    vm.validation = result.data;
                }, function (error) {
                    vm.error = error.data.ExceptionMessage;
                });
        }

        function pickTarget() {
            pickFolder(function (folder) {
                vm.options.target = folder;
            });
        }

        function pickFolder(cb) {

            editorService.open({
                size: 'small',
                section: "settings",
                treeAlias: "uSyncFiles",
                view: "views/common/infiniteeditors/treepicker/treepicker.html",
                entityType: "file",
                title: 'Pick a folder',
                isDialog: true,
                onlyInitialized: false,
                filterCssClass: "not-allowed",
                filter: i => !i.hasChildren,
                select: node => {
                    const id = decodeURIComponent(node.id.replace(/\+/g, " "));
                    cb(id);
                    editorService.close();
                },
                close: () => editorService.close()
            });
        }

        function pickHandlers() {

            editorService.open({
                title: 'Migration Handlers',
                size: 'small',
                handlers: vm.options.handlers,
                view: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/uSyncMigrations/dialogs/handlerPicker.html',
                submit: function (handlers) {
                    vm.options.handlers = handlers;
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            });
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }


        //// uSync import (we really should componentize this in the core )
        ////////////////////////////

        var modes = {
            NONE: 0,
            REPORT: 1,
            IMPORT: 2,
            EXPORT: 3
        };

        vm.savings = {};

        vm.importItems = importItems;

        function importItems(folder) {

            if (vm.working === true) return;
            vm.progress = 'importing';

            vm.results = [];
            resetStatus(modes.IMPORT);
            // getWarnings('import');

            vm.state = 'busy';

            var options = {
                action: 'import',
                group: '',
                force: false,
                set: 'default',
                folder: folder
            };

            var start = performance.now();

            performAction(options, uSync8DashboardService.importHandler)
                .then(function (results) {

                    vm.status.message = 'Post import actions';

                    uSync8DashboardService.importPost(vm.results, options, getClientId())
                        .then(function (results) {
                            vm.progress = 'imported';
                            vm.working = false;
                            vm.reported = true;
                            vm.perf = performance.now() - start;
                            group.state = 'success';
                            eventsService.emit('usync-dashboard.import.complete');
                            // calculateTimeSaved(vm.results);
                            vm.status.message = 'Complete';
                        });
                }, function (error) {
                    vm.working = false;
                    vm.state = 'error';
                    vm.error = error.data.ExceptionMessage;
                    notificationsService.error('Error', error.data.ExceptionMessage ?? error.data.exceptionMessage);
                });
        }

        function performAction(options, actionMethod, cb) {

            return $q(function (resolve, reject) {
                uSync8DashboardService.getActionHandlers(options)
                    .then(function (result) {
                        vm.status.handlers = result.data;
                        performHandlerAction(vm.status.handlers, actionMethod, options, cb)
                            .then(function () {
                                resolve();
                            }, function (error) {
                                reject(error)
                            })
                    });
            });
        }

        function performHandlerAction(handlers, actionMethod, options, cb) {


            return $q(function (resolve, reject) {

                var index = 0;
                vm.status.message = 'Starting ' + options.action;
                vm.status.total = handlers.length - 1;

                uSync8DashboardService.startProcess(options.action)
                    .then(function () {
                        runHandlerAction(handlers[index])
                    });

                function runHandlerAction(handler) {

                    vm.status.message = handler.name;

                    handler.status = 1;
                    actionMethod(handler.alias, options, getClientId())
                        .then(function (result) {

                            vm.results = vm.results.concat(result.data.actions);

                            handler.status = 2;
                            handler.changes = countChanges(result.data.actions);

                            index++;
                            vm.status.count = index;

                            if (index < handlers.length) {
                                runHandlerAction(handlers[index]);
                            }
                            else {

                                vm.status.message = 'Finishing ' + options.action;

                                uSync8DashboardService.finishProcess(options.action, vm.results)
                                    .then(function () {
                                        resolve();
                                    });
                            }
                        }, function (error) {
                            // error in this handler ? 
                            // do we want to carry on with the other ones or just stop?
                            reject(error);
                        });
                }
            });
        }

        /// resets all the flags, and messages to the start 
        function resetStatus(mode) {

            vm.fresh = false;
            vm.warnings = {};

            vm.reported = vm.showAll = false;
            vm.working = true;
            vm.showSpinner = false;
            vm.runmode = mode;
            vm.hideLink = false;
            vm.savings.show = false;

            vm.status = {
                count: 0,
                total: 1,
                message: 'Initializing',
                handlers: vm.handlers
            };

            if (!vm.hub.active) {
                vm.status.Message = 'Working ';
                vm.showSpinner = true;
            }

            vm.update = {
                message: '',
                count: 0,
                total: 1
            };

            // performance timer. 
            vm.perf = 0;


            switch (mode) {
                case modes.IMPORT:
                    vm.action = 'Import';
                    break;
                case mode.REPORT:
                    vm.action = 'Report';
                    break;
                case mode.EXPORT:
                    vm.action = 'Export';
                    break;
            }
        }


        ////
        ////// SignalR things 

        function InitHub() {
            uSyncHub.initHub(function (hub) {

                vm.hub = hub;

                vm.hub.on('add', function (data) {
                    vm.status = data;
                });

                vm.hub.on('update', function (update) {
                    vm.update = update;
                });

                vm.hub.start();
            });
        }

        function getClientId() {
            if ($.connection !== undefined) {
                return $.connection.connectionId;
            }
            return "";
        }

        function countChanges(changes) {
            var count = 0;
            angular.forEach(changes, function (val, key) {
                if (val.change !== 'NoChange') {
                    count++;
                }
            });

            return count;
        }

        var messages = [
            'Rewilding Canada',
            'Sequesting carbon footprints',
            'Chlorinating Car Pools',
            'Partitioning Social Network',
            'Estimating more than 3 weeks for migration',
            'Prelaminating Drywall Inventory',
            'Blurring Reality Lines',
            'Reticulating 3 - Dimensional Splines',
            'Preparing Captive Simulators',
            'Capacitating Genetic Modifiers',
            'Destabilizing Orbital Payloads',
            'Sequencing Cinematic Specifiers',
            'Branching Family Trees',
            'Manipulating Modal Memory'
        ];

        var messageCount = 0;

        function shuffleMessages() {
            messages = messages.sort(() => (Math.random() > 0.5) ? 1 : -1);
        }

        function doMigrationMessages() {
            messageCount = (messageCount + 1) % messages.length;
            vm.migrationMessage = messages[messageCount];
            if (vm.progress == 'migrating') {
                $timeout(function () { doMigrationMessages(); }, 2281);
            }
        }

        
    }

    angular.module('umbraco')
        .controller('uSyncMigrateController', migrateController);
})();