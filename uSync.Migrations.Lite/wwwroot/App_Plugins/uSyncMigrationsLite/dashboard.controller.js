(function () {
    'use strict';

    function dashboardController($scope,
        usyncMigrationLiteService,
        overlayService) {

        var vm = this;

        console.log('loaded');


        vm.$onInit = function () {
            usyncMigrationLiteService.hasLegacy()
                .then(function (result) {
                    if (result.data == true) {
                        
                    }
                    showLegacyOverlay();
                });
        }



        function showLegacyOverlay() {

            overlayService.open({
                view: '/App_Plugins/uSyncMigrationsLite/overlay.html',
                title: 'It looks like you are upgrading your website ?',
                closeButtonLabel: 'Cancel',
                submitButtonLabel: 'Convert',
                state: 'init',
                submit: function () {
                    // do the conversion here...
                    if (state == 'complete') {
                        // close at the end.... 
                        overlayService.close();
                    }
                },
                close: function () {
                    overlayService.close();
                }
            });


        }
    }


    angular.module('umbraco')
        .controller('uSyncMigrationsLiteDashboardController', dashboardController);
})();