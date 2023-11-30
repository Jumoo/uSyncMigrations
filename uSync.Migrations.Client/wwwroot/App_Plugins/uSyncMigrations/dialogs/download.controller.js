(function () {
    'use strict';

    function downloadController($scope,
        notificationsService,
        uSyncMigrationService) {

        var vm = this;
        vm.working = false;
        vm.download = download;

        vm.status = {};
        vm.update = {};

        vm.$onInit = function () {
            vm.hub = $scope.model.hub;

            vm.hub.on('update', function (update) {
                console.log('update', update);
                vm.update = update;
            });

            vm.hub.on('add', function (add) {
                console.log('add', add);
                vm.status = add;
            });
        }

        function getClientId() {
            if ($.connection !== undefined) {
                return $.connection.connectionId;
            }
            return "";
        }

        vm.close = function () {
            $scope.model.close();
        }

        function download() {

            vm.working = true;

            uSyncMigrationService.download(getClientId())
                .then(function () {
                    vm.working = false;
                    console.log('done');
                    vm.close();
                }, function (error) {
                    notificationsService.error('failed', 'failed to download, check console');
                    console.log(error);
                });
        }
    }

    angular.module('umbraco')
        .controller('uSyncDownloadMigrationController', downloadController)
})();