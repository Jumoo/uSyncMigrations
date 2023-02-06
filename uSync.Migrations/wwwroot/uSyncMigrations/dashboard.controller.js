(function () {
    'use strict';

    function dashboardController($q,
        uSyncMigrationService, 
        editorService, notificationsService) {

        var vm = this;
        vm.view = "groups";

        vm.include = [];

        vm.working = false;
        vm.converted = false;

        vm.state = 'init';

        vm.loadProfiles = loadProfiles;
        vm.start = start;

        vm.groups = [
            {
                alias: 'legacy',
                name: "Migrate Legacy",
                description: "Migrate settings and content from your old (Umbraco 7) sites",
                icon: "icon-shift color-purple"
            },
            {
                alias: 'convert',
                name: "Convert existing",
                description: "Convert some of your existing data to newer properties",
                icon: "icon-conversation color-blue"
            }
        ];

        // start 
        vm.$onInit = function () {

        //    var p = [];

        //    p.push(uSyncMigrationService.getProfiles()
        //        .then(function (result) {
        //            vm.profiles = result.data.profiles;
        //            vm.hasCustom = result.data.hasCustom;
        //        }))

        //    $q.all(p)
        //        .then(function () {
        //            vm.loading = false;
        //        });

            vm.loading = false;
        }


        function loadProfiles(group) {
            vm.view = 'profiles';
            vm.group = group;

            uSyncMigrationService.getProfiles(group.alias)
                .then(function (result) {
                    vm.profiles = result.data;
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
