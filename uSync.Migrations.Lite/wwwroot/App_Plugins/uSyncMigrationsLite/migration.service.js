(function () {
    'use strict';

    function migrationService($http) {

        var serviceRoot = Umbraco.Sys.ServerVariables.uSyncMigrationsLite.conversionService;

        return {
            hasLegacy: hasLegacy
        };

        function hasLegacy() {
            return $http.get(serviceRoot + "LegacyItemCheck");
        }
    }

    angular.module('umbraco')
        .factory('usyncMigrationLiteService', migrationService);
})();