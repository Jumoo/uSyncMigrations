(function () {
    'use strict';

    var migrationMessagesComponent = {
        templateUrl: Umbraco.Sys.ServerVariables.application.applicationPath + 'App_Plugins/uSyncMigrations/components/migrationMessages.html',
        bindings: {
            working: '<'
        },
        controllerAs: 'vm',
        controller: migrationMessageController
    };

    function migrationMessageController($timeout) {

        var vm = this;

        vm.$onInit = function () {
            shuffleMessages();
            doMigrationMessages();
        }

        vm.migrationMessage = '';

        var messages = [
            'Rewilding Canada',
            'Sequesting carbon footprints',
            'Chlorinating Car Pools',
            'Partitioning Social Network',
            'Prelaminating Drywall Inventory',
            'Blurring Reality Lines',
            'Reticulating 3 - Dimensional Splines',
            'Preparing Captive Simulators',
            'Capacitating Genetic Modifiers',
            'Destabilizing Orbital Payloads',
            'Sequencing Cinematic Specifiers',
            'Branching Family Trees',
            'Manipulating Modal Memory'
        ];

        var messageCount = 0;

        function shuffleMessages() {
            messages = messages.sort(() => (Math.random() > 0.5) ? 1 : -1);
        }

        function doMigrationMessages() {
            messageCount = (messageCount + 1) % messages.length;
            vm.migrationMessage = messages[messageCount];
            if (vm.working) {
                $timeout(function () { doMigrationMessages(); }, 2281);
            }
        }
    }

    angular.module('umbraco')
        .component('usyncMigrationMessages', migrationMessagesComponent);

})();