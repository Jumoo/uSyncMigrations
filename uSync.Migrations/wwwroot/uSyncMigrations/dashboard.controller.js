(function () {
    'use strict';

    function dashboardController($q,
        uSyncMigrationService, 
        editorService, notificationsService) {

        var vm = this;
        vm.options = {
            hasPending: false,
            handlers: []
        };

        vm.include = [];

        vm.working = false;
        vm.converted = false;

        vm.state = 'init';

        vm.migrate = migrate;
        vm.start = start;

        // start 
        vm.$onInit = function () {

            var p = [];

            p.push(uSyncMigrationService.getMigrationOptions()
                .then(function (result) {
                    vm.options = result.data;
                }));

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
