(function () {
    'use strict';

    var uSyncMigrationResultsComponent = {
        templateUrl: Umbraco.Sys.ServerVariables.application.applicationPath + 'App_Plugins/uSyncMigrations/components/migrationResults.html',
        bindings: {
            results: '<',
            action: '@',
            showAll: '<',
            isValid: '='
        },
        controllerAs: 'vm',
        controller: uSyncMigrationResultController
    };

    function uSyncMigrationResultController($scope, usyncMigrationHelpers)
    {
        var vm = this;

        vm.$onInit = function () {
            checkMessages();
        }

        $scope.$watch('vm.results', function (newValue) {
            if (newValue != undefined && newValue != null) {
                checkMessages(newValue);
            }
        });

        vm.counts = {
            Success: 0,
            Error: 0,
            Warning: 0
        };

        function checkMessages() {

            vm.counts.Success = vm.counts.Error = vm.counts.Warning = 0;

            if (vm.results != undefined && vm.results != null) {

                vm.results.messages.forEach(e => {
                    vm.counts[e.messageType]++;
                });

                vm.hasError = vm.counts.Error > 0;
            }

            if (vm.counts.Error + vm.counts.Warning == 0) {
                vm.showAll = true;
            }
        }

        vm.copyReport = copyReport;
        function copyReport() {
            usyncMigrationHelpers.copyResults(vm.results.messages);
         
        }

       

    }

    angular.module('umbraco')
        .component('usyncMigrationResults', uSyncMigrationResultsComponent);
})();