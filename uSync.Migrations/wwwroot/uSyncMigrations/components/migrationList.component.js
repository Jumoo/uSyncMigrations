(function() {
    'use strict';

    var uSyncMigrationListComponent = {
        templateUrl: Umbraco.Sys.ServerVariables.application.applicationPath + 'App_Plugins/uSyncMigrations/components/migrationList.html',
        bindings: {
            onSelect: '&',
            onRemove: '&'
        },
        controllerAs: 'vm',
        controller: uSyncMigrationlistController
    };

    function uSyncMigrationlistController($scope,
        overlayService,
        uSyncMigrationService) {

        var vm = this;
        vm.loading = true;

        vm.select = select;
        vm.remove = remove;

        vm.$onInit = function () {
            vm.loading = false;
            loadMigrations();
        }

        function loadMigrations() {
            uSyncMigrationService.getMigrations()
                .then(function (results) {
                    vm.loading = false;
                    vm.migrations = results.data;
                });
        }

        function select(migration) {
            vm.onSelect({ migration: migration });
        }

        function remove(migration)
        {
            overlayService.confirm({
                title: 'Remove migration',
                content: 'Removing a migration will delete the files from disk',
                submitButtonLabelKey: 'general_remove',
                closeButtonLabelKey: 'general_cancel',
                confirmType: 'delete',
                submit: function () {
                    uSyncMigrationService.deleteMigration(migration.id)
                        .then(function (result) {
                            loadMigrations();
                        });
                    overlayService.close();
                }
            });

        }

    }

    angular.module('umbraco')
        .component('usyncMigrationList', uSyncMigrationListComponent);
})();