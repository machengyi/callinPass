angular.module('App')
.controller('PassList', function ($scope, $http, $state) {
    var req_json = {};//自定义JSON数据
    //加载标题（登录者名称）
    $scope.getTitle = function () {
        req_json.func = 'TransitionList'
        req_json.tabName = 'title';
        $http({
            method: 'POST',
            url: '../../interface/getDataInterface.aspx',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' },
            data: req_json
        })
        .then(function (result) {
            $scope.passListTitle = result.data.name.trim();
            $scope.getData();
        });
    };
    //加载pass清单
    $scope.getData = function () {
        req_json.func = 'TransitionList'
        req_json.tabName = 'content';
        $http({
            method: 'POST',
            url: '../../interface/getDataInterface.aspx',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' },
            data: req_json
        })
        .then(function (result) {
            if (result.data == "102") {
                $scope.errorInfo = "没有需要pass的数据";
            } else {
                $scope.errorInfo = "";
                $scope.passList = result.data;
            }
           
        });
    };
    //页面跳转方法
    $scope.pageJump = function (customerName, customerNo, Date) {
        $state.go('tabs.passContent', { customerName: customerName, customerNo: customerNo, Date: Date, id: 2 });
        //$location.path('/tabs/passContent')
    };

    $scope.getTitle();
});