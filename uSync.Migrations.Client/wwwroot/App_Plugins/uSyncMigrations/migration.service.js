(function () {
    'use strict';

    function migrationService($http, $q) {

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
            saveStatus: saveStatus,

            download: download,
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

        function download(clientId) {
            return downloadPost(serviceRoot + 'Download?clientId=' + clientId);
        }

        /*
        * Downloads a file to the client using AJAX/XHR
        * Based on an implementation here: web.student.tuwien.ac.at/~e0427417/jsdownload.html
        * See https://stackoverflow.com/a/24129082/694494
        */
        function downloadPost(httpPath, payload) {

            // Use an arraybuffer
            return $http.post(httpPath, payload, { responseType: 'arraybuffer' })
                .then(function (response) {

                    var octetStreamMime = 'application/octet-stream';
                    var success = false;

                    // Get the headers
                    var headers = response.headers();

                    // Get the filename from the header or default to "download.bin"
                    var filename = getFileName(headers);

                    // Determine the content type from the header or default to "application/octet-stream"
                    var contentType = headers['content-type'] || octetStreamMime;

                    try {
                        // Try using msSaveBlob if supported
                        let blob = new Blob([response.data], { type: contentType });
                        if (navigator.msSaveBlob)
                            navigator.msSaveBlob(blob, filename);
                        else {
                            // Try using other saveBlob implementations, if available
                            var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
                            if (saveBlob === undefined) throw "Not supported";
                            saveBlob(blob, filename);
                        }
                        success = true;
                    } catch (ex) {
                        console.log("saveBlob method failed with the following exception:");
                        console.log(ex);
                    }

                    if (!success) {
                        // Get the blob url creator
                        var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
                        if (urlCreator) {
                            // Try to use a download link
                            var link = document.createElement('a');
                            if ('download' in link) {
                                // Try to simulate a click
                                try {
                                    // Prepare a blob URL
                                    let blob = new Blob([response.data], { type: contentType });
                                    let url = urlCreator.createObjectURL(blob);
                                    link.setAttribute('href', url);

                                    // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
                                    link.setAttribute("download", filename);

                                    // Simulate clicking the download link
                                    var event = document.createEvent('MouseEvents');
                                    event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
                                    link.dispatchEvent(event);
                                    success = true;

                                } catch (ex) {
                                    console.log("Download link method with simulated click failed with the following exception:");
                                    console.log(ex);
                                }
                            }

                            if (!success) {
                                // Fallback to window.location method
                                try {
                                    // Prepare a blob URL
                                    // Use application/octet-stream when using window.location to force download
                                    let blob = new Blob([response.data], { type: octetStreamMime });
                                    let url = urlCreator.createObjectURL(blob);
                                    window.location = url;
                                    success = true;
                                } catch (ex) {
                                    console.log("Download link method with window.location failed with the following exception:");
                                    console.log(ex);
                                }
                            }

                        }
                    }

                    if (!success) {
                        // Fallback to window.open method
                        window.open(httpPath, '_blank', '');
                    }

                    return $q.resolve();

                }, function (response) {

                    return $q.reject({
                        errorMsg: "An error occurred downloading the file",
                        data: response.data,
                        status: response.status
                    });
                });
        }


        function getFileName(headers) {
            var disposition = headers["content-disposition"];
            if (disposition && disposition.indexOf('attachment') !== -1) {
                var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                var matches = filenameRegex.exec(disposition);
                if (matches != null && matches[1]) {
                    return matches[1].replace(/['"]/g, '');
                }
            }

            return "usync_pack.zip";
        }

    }

    angular.module('umbraco').factory('uSyncMigrationService', migrationService);
})();
