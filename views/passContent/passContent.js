﻿angular.module('App')
.controller('PassContent', function ($scope, $http, $ionicTabsDelegate, $stateParams) {

    //加载初始数据
    $scope.load = function () {
       
        //$http({
        //    method: "POST",
        //    url: '../../interface/getDataInterface.aspx',
        //    headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' }
        //})
        //.then(function (result) {

        //}).finally(function () {
        //    $scope.$broadcast('scroll.infiniteScrollComplete');//广播scroll.refreshComplete事件，这样ionRefresher知道何时关闭
        //});
    }
    
    //手指向左滑
    $scope.onSwipeLeft = function () {
        $ionicTabsDelegate.select(1)
    }

    $scope.load();
})