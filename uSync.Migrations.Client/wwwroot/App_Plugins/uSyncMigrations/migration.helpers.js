(function () {
    'use strict';

    function migrationHelpers(notificationsService) {

        return {
            copyResults: copyResults,
            notifyError: notifyError
        };

        function copyResults(results) {

            if (results.length > 0) {
                var report = objectKeys(results[0]).toString() + '\n'
            }

            results.forEach(function (message) {
                var arr = objectToArray(message);
                report += arr.toString() + '\n';
            });

            navigator.clipboard.writeText(report);
            notificationsService.success('copied', 'report copied to clipboard');

        }


        function objectKeys(obj) {

            var arr = [];
            for (let k in obj) {
                if (Object.prototype.hasOwnProperty.call(obj, k)) {
                    if (!k.startsWith('$$')) {
                        arr.push(k);
                    }
                }
            }

            return arr;
        }

        function objectToArray(obj) {
            var arr = [];
            for (let p in obj) {
                if (Object.prototype.hasOwnProperty.call(obj, p)) {
                    if (!p.startsWith('$$')) {
                        arr.push(obj[p]);
                    }
                }
            }

            return arr;

        }

        function notifyError(methodName, error) {
            console.error('Error', methodName, error);

            try {
                const msg = error.data.ExceptionMessage ?? error.data.exceptionMessage;
                notificationsService.error(`error ${methodName}`, msg);
                return msg;
            }
            catch {
                notificationsService.error(`error ${methodName}`, 'there was an error check the browser logs');
                return JSON.stringify(error, null, 1);
            }
        }
    }


    angular.module('umbraco').factory('usyncMigrationHelpers', migrationHelpers);

})();