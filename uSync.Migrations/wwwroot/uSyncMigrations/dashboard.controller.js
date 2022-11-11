(function () {
    'use strict';

    function dashboardController(uSyncMigrationService,
        notificationsService) {

        var vm = this;
        vm.options = {
            hasPending: false,
            handlers: []
        };

        vm.include = [];

        vm.working = true;
        vm.converted = false;

        vm.state = 'init';

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
                handlers: vm.options.handlers
            }

            vm.state = 'busy';

            // do the migration...
            uSyncMigrationService.migrate(options)
                .then(function (result) {
                    vm.results = result.data;
                    vm.converted = true;
                    vm.state = 'success';
                }, function (error) {
                    vm.state = 'error';
                    console.log(error)
                    notificationsService.error('error', error.data.ExceptionMessage);
                });

        }

    }

    angular.module('umbraco').controller('uSyncMigrationDashboardController', dashboardController);
})();
