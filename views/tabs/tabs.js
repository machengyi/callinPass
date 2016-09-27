angular.module('App')
.controller('Tabs', function ($scope, $http, $stateParams) {
    $scope.customerName = $stateParams.customerName;
    $scope.passDate = $stateParams.Date;
    //如果是从pass清单进入则出现返回按钮
    if ($stateParams.id == 1) {
        $scope.returnButtonShow = true;
    } else {
        $scope.returnButtonShow = false;
    }
})