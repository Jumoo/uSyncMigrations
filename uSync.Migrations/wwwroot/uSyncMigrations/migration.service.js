(function () {
    'use strict';

    function migrationService($http) {

        var serviceRoot = Umbraco.Sys.ServerVariables.uSyncMigrations.migrationService;

        return {
            hasPending: hasPending,
            getMigrationOptions: getMigrationOptions,
            migrate: migrate
        };

        // 
        function hasPending() {
            return $http.get(serviceRoot + "HasPendingMigration");
        }

        function getMigrationOptions() {
            return $http.get(serviceRoot + "GetMigrationOptions");
        }

        function migrate(options) {
            return $http.post(serviceRoot + "Migrate", options);
        }
    }

    angular.module('umbraco')
        .factory('uSyncMigrationService', migrationService);

})();