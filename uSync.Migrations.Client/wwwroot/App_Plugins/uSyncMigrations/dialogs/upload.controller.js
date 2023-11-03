(function () {
    'use strict';

    function uploadController($scope, Upload, notificationsService,
        overlayService, uSyncMigrationService) {

        var vm = this;

        vm.uploaded = false; 

        // file stuff.
        vm.file = null;

        // methods 
        vm.uploadChange = uploadChange;
        vm.upload = upload;

        function uploadChange(files, event) {
            if (files && files.length > 0) {
                vm.file = files[0];
            }
        }

        function upload(file) {
            vm.buttonState = 'busy';

            Upload.upload({
                url: Umbraco.Sys.ServerVariables.uSyncMigrations.migrationService + 'Upload',
                file: file
            }).success(function (data, status, headers, config) {
                vm.uploaded = true;
                vm.success = data.success;

                if (data.success) {
                    vm.buttonState = 'success';
                    vm.status = data.status;
                }
                else {
                    vm.buttonState = 'error';
                    vm.errors = data.errors;
                }
            }).error(function (event, status, headers, config) {
                vm.uploaded = true;
                vm.success = false;
                vm.buttonState = 'error';
                notificationsService.error('error', 'Failed to upload '
                    + status + ' ' + event.ExceptionMessage);

                vm.errors.push('Zip file upload error ' +
                    '[' + status + '] ' +
                    event.ExceptionMessage);
            });
        }


        /// dialog 
        vm.submit = function () {

            if (vm.status) {
                uSyncMigrationService.saveStatus(vm.status)
                    .then(function (result) {
                        $scope.model.submit(vm.status);
                    });
            }
            else {
                $scope.model.submit(vm.status);
            }
        }

        vm.close = function () {

            if (vm.status) {
                overlayService.confirm({
                    title: 'Remove migration',
                    content: 'Removing a migration will delete the files from disk',
                    submitButtonLabelKey: 'general_remove',
                    closeButtonLabelKey: 'general_cancel',
                    confirmType: 'delete',
                    submit: function () {
                        uSyncMigrationService.deleteMigration(vm.status.id)
                            .then(function (result) {
                                $scope.model.close();
                            });
                        overlayService.close();
                    }
                });
            }
            else {
                $scope.model.close();
            }
        }
    }


    angular.module('umbraco')
        .controller('uSyncUploadMigrationController', uploadController);

})();