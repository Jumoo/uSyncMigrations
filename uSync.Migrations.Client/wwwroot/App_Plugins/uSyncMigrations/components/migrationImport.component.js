(function () {

    var importComponent = {
        templateUrl: Umbraco.Sys.ServerVariables.application.applicationPath + 'App_Plugins/uSyncMigrations/components/migrationImport.html',
        bindings: {
            folder: '<',
            migrationStatus: '=',
            onImport: '&',
            onReport: '&'
        },
        controllerAs: 'vm',
        controller: importController,
    };

    function importController($q, eventsService,
        notificationsService, uSyncHub, uSync8DashboardService,
        uSyncMigrationService) {

        var vm = this;
        vm.loading = true;

        vm.doImport = doImport;
        vm.doReport = doReport;

        vm.$onInit = function () {

            getHandlerGroups();
            InitHub();
        }

        // work out what import options we have 
        function getHandlerGroups() {

            vm.groups = [];

            uSync8DashboardService.getHandlerGroups('default')
                .then(function (result) {
                    vm.loading = false;

                    var groups = result.data;

                    _.forEach(groups, function (icon, group) {

                        if (group == '_everything') {
                            vm.groups.push({
                                icon: 'icon-paper-plane-alt color-deep-orange',
                                name: 'Everything',
                                group: '',
                                state: 'init',
                                key: 'everything'
                            });
                        }
                        else {
                            vm.groups.push({
                                name: group,
                                group: group,
                                icon: icon,
                                key: group.toLowerCase(),
                                state: 'init'
                            });
                        }
                    });
                });
        }


        function doImport(group) {

            if (vm.onImport) {
                vm.onImport();
            }

            importItems(group);
        }

        function doReport(group) {
            if (vm.onReport) {
                vm.onReport();
            }

            report(group);
        }

        function complete(group) {

            if (group.key == 'everything') {
                _.forEach(vm.groups, function (group) {

                    if (group.key != 'everything') {
                        vm.migrationStatus.importStatus[group.group] = true;
                    }
                });
            }
            else {
                vm.migrationStatus.importStatus[group.group] = true;
            }

            uSyncMigrationService.saveStatus(vm.migrationStatus)
                .then(function (result) {
                    console.log('saved status');
                });

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

        vm.working = false;
        vm.reported = false;
        vm.state = 'init';
        vm.progress = 'init';
        vm.importItems = importItems;
        vm.report = report;


        function report(group) {

            if (vm.working === true) return;
            vm.progress = 'reporting';

            vm.results = [];

            resetStatus(modes.REPORT);
            // getWarnings('report');
            group.state = 'busy';

            var options = {
                action: 'report',
                group: group.group,
                set: vm.currentSet,
                folder: vm.folder,
            };

            var start = performance.now();

            performAction(options, uSync8DashboardService.reportHandler)
                .then(function (results) {
                    vm.working = false;
                    vm.reported = true;
                    vm.perf = performance.now() - start;
                    vm.status.message = 'Report complete';
                    group.state = 'success';

                    vm.progress = 'completed';
                }, function (error) {
                    vm.working = false;
                    group.state = 'error';
                    notificationsService.error('Error', error.data.ExceptionMessage ?? error.data.exceptionMessage);
                });
        }

        function importItems(group) {

            var folder = vm.folder; 

            if (vm.working === true) return;
            vm.progress = 'importing';

            vm.results = [];
            resetStatus(modes.IMPORT);
            // getWarnings('import');

            vm.state = 'busy';

            var options = {
                action: 'import',
                group: group.group,
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
                            vm.working = false;
                            vm.reported = true;
                            vm.perf = performance.now() - start;
                            group.state = 'success';
                            eventsService.emit('usync-dashboard.import.complete');
                            vm.status.message = 'Complete';

                            vm.progress = 'completed';
                            complete(group);
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
    }

    angular.module('umbraco')
        .component('usyncMigrationImport', importComponent);

})();