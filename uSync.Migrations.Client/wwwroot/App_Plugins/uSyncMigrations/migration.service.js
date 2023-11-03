(function () {
    'use strict';

    function migrationService($http) {

        var serviceRoot = Umbraco.Sys.ServerVariables.uSyncMigrations.migrationService;

        return {
            hasPending: hasPending,
            getMigrationOptions: getMigrationOptions,
            migrate: migrate,
            getProfiles: getProfiles,
            getProfilesByVersion: getProfilesByVersion,
            validate: validate,
            detectVersion: detectVersion,
            getDefaultTarget: getDefaultTarget,
            getDefaultProfile: getDefaultProfile,
            getConversionDefaults: getConversionDefaults,
            getPreferedMigrators: getPreferedMigrators,

            getMigrations: getMigrations,
            deleteMigration: deleteMigration,
            saveStatus: saveStatus
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

        function getProfiles(groupAlias) {
            return $http.get(serviceRoot + "GetProfiles/?groupAlias=" + groupAlias);
        }

        function getProfilesByVersion(version) {
            return $http.get(serviceRoot + "GetProfilesByVersion/?version=" + version);
        }

        function getConversionDefaults() {
            return $http.get(serviceRoot + "GetConversionDefaults");
        }

        function detectVersion(folder) {
            return $http.get(serviceRoot + 'DetectVersion?folder=' + folder);
        }

        function getDefaultProfile(version) {
            return $http.get(serviceRoot + 'GetDefaultProfile?version=' + version);
        }

        function getDefaultTarget(version) {
            return $http.get(serviceRoot + 'GetDefaultTarget?version=' + version);
        }

        function getPreferedMigrators(planName) {
            return $http.get(serviceRoot + 'GetPreferedMigrators?planName=' + planName);
        }

        function validate(options) {
            return $http.post(serviceRoot + "Validate", options);
        }

        function getMigrations() {
            return $http.get(serviceRoot + 'GetMigrations');
        }

        function deleteMigration(id) {
            return $http.delete(serviceRoot + 'DeleteMigration?id=' + id);
        }

        function saveStatus(status) {
            return $http.post(serviceRoot + 'SaveStatus', status);
        }
    }

    angular.module('umbraco').factory('uSyncMigrationService', migrationService);
})();
