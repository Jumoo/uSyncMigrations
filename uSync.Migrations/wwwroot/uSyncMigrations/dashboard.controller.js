(function () {
    'use strict';

    function dashboardController($q,
        uSyncMigrationService, 
        editorService, notificationsService) {

        var vm = this;
        vm.include = [];

        vm.working = false;
        vm.converted = false;

        vm.state = 'init';

        vm.start = start;

        // start 
        vm.$onInit = function () {

            var p = [];

            p.push(uSyncMigrationService.getProfiles()
                .then(function (result) {
                    vm.profiles = result.data.profiles;
                    vm.hasCustom = result.data.hasCustom;
                }))

            $q.all(p)
                .then(function () {
                    vm.loading = false;
                });
        }


        function start(profile) {

            vm.working = true;

            editorService.open({
                title: 'Start migration',
                size: 'medium',
                profile: profile,
                view: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/uSyncMigrations/migrate.html',
                submit: function (action) {

                },
                close: function () {
                    vm.working = false;
                    editorService.close();
                }
            });
        }
    }

    angular.module('umbraco').controller('uSyncMigrationDashboardController', dashboardController);
})();
