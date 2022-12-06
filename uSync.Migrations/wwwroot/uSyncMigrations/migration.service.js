(function () {
    'use strict';

    function migrationService($http) {

        var serviceRoot = Umbraco.Sys.ServerVariables.uSyncMigrations.migrationService;

        return {
            hasPending: hasPending,
            getMigrationOptions: getMigrationOptions,
            migrate: migrate,
            getProfiles: getProfiles,
            validateSource: validateSource,
            validate: validate
        };

        function hasPending() {
            return $http.get(serviceRoot + "HasPendingMigration");
        }

        function getMigrationOptions(version) {
            return $http.get(serviceRoot + "GetMigrationOptions?version=" + version);
        }

        function migrate(options) {
            return $http.post(serviceRoot + "Migrate", options);
        }

        function getProfiles() {
            return $http.get(serviceRoot + "GetProfiles");
        }

        function validateSource(version, source) {
            return $http.get(serviceRoot + "ValidateSource/?version=" + version + "&source=" + source);
        }

        function validate(options) {
            return $http.post(serviceRoot + "Validate", options);
        }
    }

    angular.module('umbraco').factory('uSyncMigrationService', migrationService);
})();
