(function () {
    'use strict';

    function dashboardController(uSyncMigrationService) {

        var vm = this;
        vm.options = {
            hasPending: false,
            handlers: []
        };

        vm.include = [];

        vm.working = true;
        vm.converted = false;

        vm.migrate = migrate;

        // start 
        vm.$onInit = function () {
            uSyncMigrationService.getMigrationOptions()
                .then(function (result) {
                    vm.options = result.data;
                });
        }

        // actions
        function migrate() {

            var options = {
                handlers : vm.options.handlers
            }

            // do the migration...
            uSyncMigrationService.migrate(options)
                .then(function (result) {
                    vm.results = result.data;
                    vm.converted = true;
                });

        }

    }

    angular.module('umbraco')
        .controller('uSyncMigrationDashboardController', dashboardController);
})();