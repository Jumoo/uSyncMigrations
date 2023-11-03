(function () {
    'use strict';

    function handlerPickerController($scope) {

        var vm = this;

        vm.handlers = $scope.model.handlers;

        vm.submit = function () {
            if ($scope.model.submit) {
                $scope.model.submit(vm.handlers);
            }
        }

        vm.close = function () {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }
    }

    angular.module('umbraco')
        .controller('uSyncMigrationHandlerPickerController', handlerPickerController);

})();