(function () {

    'use strict';

    function filePickerController($scope, uSyncMigrationService, Upload, notificationsService) {

        var vm = this;
        vm.status = $scope.model.status; 
        vm.isNew = $scope.model.isNew;

        vm.isValid = isValid;

        vm.$onInit = function () {

            if (vm.status === undefined) {

                vm.status = {

                };
            }
        }
           
        function isValid() {

            if (vm.status.name !== undefined && vm.status.name.length < 1) { return false; }
            if (vm.status.version !== undefined && vm.status.version < 7) { return false; }
            if (vm.status.source === undefined) { return false; }

            return true;

        }


        /// dialog 
        vm.submit = function () {

            if (isValid()) {
                uSyncMigrationService.saveStatus(vm.status)
                    .then(function (result) {
                        $scope.model.submit(result.data);
                    });
            }
            else {
                $scope.model.submit(vm.status);
            }
        }

        vm.close = function () {

            if ($scope.model.close) {
                $scope.model.close();
            }
        }

    }

    angular.module('umbraco')
        .controller('uSyncFilePickerController', filePickerController);

})();