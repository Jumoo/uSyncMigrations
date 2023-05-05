(function () {
    'use strict';

    function dashboardController($q,
        uSyncMigrationService, uSync8DashboardService,
        editorService, notificationsService, navigationService) {

        var vm = this;

        vm.pickSource = pickSource;
        vm.uploadZip = uploadZip;
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

        function validate(status) {
            uSyncMigrationService.validate(status)
                .then(function (result) {
                    vm.validation = result.data;
                    vm.sourceValid = vm.validation.success;
                }, function (error) {
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

            uSyncMigrationService.migrate(vm.migrationStatus)
                .then(function (result) {
                    vm.state = 'success';
                    vm.step = 'migrated';
                    vm.migrationResults = result.data;
                    vm.migrationStatus.migrated = true;
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
    }

    angular.module('umbraco').controller('uSyncMigrationDashboardController', dashboardController);
})();
