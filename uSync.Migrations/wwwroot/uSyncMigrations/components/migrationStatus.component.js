(function () {

    var migrationStatusComponent = {
        templateUrl: Umbraco.Sys.ServerVariables.application.applicationPath + 'App_Plugins/uSyncMigrations/components/migrationStatus.html',
        bindings: {
            readonly: '<',
            isNew: '<',
            status: '=',
        },
        controllerAs: 'vm',
        controller: statusController
    };

    function statusController($scope, editorService,
        uSyncMigrationService) {

        var vm = this;

        vm.pickSource = pickSource;
        vm.pickTarget = pickTarget;
        vm.pickSiteFolder = pickSiteFolder;

        vm.$onInit = function () {
            if (vm.status.version) {
                getPlans(vm.status.version);
            }
        }

        ////

        function pickSource() {
            pickFolder(function (folder) {
                vm.status.source = folder;
                detectVersion(vm.status.source);
            });
        }

        function pickSiteFolder() {
            pickFolder(function (folder) {
                vm.status.siteFolder = folder;
            });
        }

        function pickTarget() {
            pickFolder(function (folder) {
                vm.status.target = folder;
            });
        }

        function pickFolder(cb) {

            editorService.open({
                size: 'small',
                section: "settings",
                treeAlias: "uSyncFiles",
                view: "views/common/infiniteeditors/treepicker/treepicker.html",
                entityType: "file",
                title: 'Pick a folder',
                isDialog: true,
                onlyInitialized: false,
                filterCssClass: "not-allowed",
                filter: i => !i.hasChildren,
                select: node => {
                    const id = decodeURIComponent(node.id.replace(/\+/g, " "));
                    cb(id);
                    editorService.close();
                },
                close: () => editorService.close()
            });
        }

        //
        function detectVersion(folder) {
            uSyncMigrationService.detectVersion(folder)
                .then(function (result) {
                    if (vm.status.version != result.data) {
                        vm.status.version = result.data;

                        getPlans(vm.status.version);
                        if (vm.isNew) {
                            getTarget(vm.status.version);
                            getDefaultProfile(vm.status.version);
                        }
                    }
                });
        }

        function getTarget(version) {
            uSyncMigrationService.getDefaultTarget(version)
                .then(function (result) {
                    vm.status.target = result.data;
                });
        }

        function getDefaultProfile(version) {
            uSyncMigrationService.getDefaultProfile(version)
                .then(function (result) {
                    vm.status.plan = result.data;
                });
        }

        function getPlans(version) {
            uSyncMigrationService.getProfilesByVersion(version)
                .then(function (result) {
                    vm.plans = result.data;
                });
        }
    }

    angular.module('umbraco')
        .component('usyncMigrationStatus', migrationStatusComponent);


})();