(function () {
    'use strict';

    function dashboardController($q,
        uSyncMigrationService, uSyncHub,
        editorService, notificationsService, navigationService) {

        var vm = this;

        vm.pickSource = pickSource;
        vm.uploadZip = uploadZip;
        vm.download = download;
        vm.edit = edit;

        vm.canBeImported = canBeImported;
        vm.importing = importing;
        vm.reporting = reporting;

        vm.startNew = startNew;
        vm.existing = existing;
        vm.selectMigration = selectMigration;
        vm.options = { sourceVersion : 7 };
        vm.sourceValid = false;
        vm.step = 'init';

        vm.migrate = migrate;

        vm.$onInit = function () {
            vm.loading = false;
            InitHub();
            navigationService.syncTree({ tree: "uSyncMigrations", path: "-1" });
        }

        function startNew() {
            vm.step = 'new';
        }

        function existing() {
            uSyncMigrationService.getConversionDefaults()
                .then(function (result) {
                    openStatus(result.data);
                });
        }


        function openStatus(existing) {

            editorService.open({

                title: 'Migration source',
                size: 'medium',
                isNew: existing == undefined,
                status: existing,
                view: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/uSyncMigrations/dialogs/filePicker.html',
                submit: function (migrationStatus) {
                    vm.migrationStatus = migrationStatus;
                    vm.step = 'start';
                    editorService.close();
                    validate(vm.migrationStatus);
                },
                close: function () {
                    editorService.close();
                }
            });
        }

        function edit() {
            openStatus(vm.migrationStatus);
        }

        function pickSource() {
            openStatus();
        }

        function uploadZip() {
            editorService.open({
                title: 'Pick Source',
                size: 'medium',
                view: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/uSyncMigrations/dialogs/upload.html',
                submit: function (migrationStatus) {
                    vm.migrationStatus = migrationStatus;
                    vm.step = 'start';
                    editorService.close();
                    validate(vm.migrationStatus);
                },
                close: function () {
                    editorService.close();
                }
            });
        }

        function download() {
            editorService.open({
                title: 'download',
                size: 'small',
                view: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/uSyncMigrations/dialogs/download.html',
                hub: vm.hub,
                close: function () {
                    editorService.close();
                }
            });
        }


        function validate(status) {
            vm.validating = true;
            uSyncMigrationService.validate(status)
                .then(function (result) {
                    vm.validating = false; 
                    vm.validation = result.data;
                    vm.sourceValid = vm.validation.success;
                }, function (error) {
                    vm.validating = false; 
                    vm.error = error.data.ExceptionMessage;
                });
        }


        function selectMigration(migration) {
            vm.step = 'start';
            vm.migrationStatus = migration;
            validate(vm.migrationStatus);
        }


        /// migration bit .
        ///

        function migrate() {
            vm.step = 'migrating';
            vm.working = true;

            vm.migrationStatus.clientId = getClientId();

            uSyncMigrationService.migrate(vm.migrationStatus)
                .then(function (result) {
                    vm.state = 'success';
                    vm.step = 'migrated';
                    vm.migrationResults = result.data;
                    vm.migrationStatus.migrated = true;
                    vm.migrationStatus.success = result.success;
                    vm.working = false;
                }, function (error) {
                    vm.state = 'error';
                    vm.working = false;
                    vm.error = error.data.ExceptionMessage;
                    notificationsService.error('error', error.data.ExceptionMessage);
                });
        }

        function canBeImported() {
            return (vm.step != 'migrating' && vm.migrationStatus?.migrated == true);
        }

        function importing() {
            vm.step = 'importing';
        }

        function reporting() {
            vm.step = 'importing';
        }

        vm.goBack = goBack;

        function goBack() {
            if (vm.step == 'start') {
                vm.step = 'init';
            }
            else {
                vm.step = 'start';
            }
        }

        ////// SignalR things 
        function InitHub() {
            uSyncHub.initHub(function (hub) {

                vm.hub = hub;

                vm.hub.on('add', function (data) {
                    vm.status = data;
                    console.log('add', data);
                });

                vm.hub.on('update', function (update) {
                    console.log('update', update);

                    var percentage = 0;
                    if (vm.update !== undefined && vm.update.percentage !== undefined) {
                        percentage = vm.update.percentage;
                    }

                    vm.update = update;

                    if (update.total > 0) {
                        vm.update.percentage = Math.round((update.count / update.total) * 100);
                    }
                    else {
                        vm.update.percentage = percentage;
                    }
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
    }

    angular.module('umbraco').controller('uSyncMigrationDashboardController', dashboardController);
})();
