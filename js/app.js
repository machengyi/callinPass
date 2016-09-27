angular.module("App", ['ionic'])
.config(function ($stateProvider, $urlRouterProvider) {
    $stateProvider.state('tabs', {
        url: '/tabs/:customerName/:Date/:id',
        abstract: true,
        controller:'Tabs',
        templateUrl: 'views/tabs/tabs.html'
    })
    .state('tabs.passContent', {
        url: '/passContent/:customerNo',
        views: {
            'passContent-tab': {
                templateUrl: 'views/passContent/passContent.html',
                controller: 'PassContent'
            }
        }
    })
    .state('tabs.infoContent', {
        url: '/infoContent',
        views: {
            'infoContent-tab': {
                templateUrl: 'views/infoContent/infoContent.html',
                controller: 'InfoContent'
            }
        }
    })
    .state('tabs.assignContent', {
        url: '/assignContent',
        views: {
            'assignContent-tab': {
                templateUrl: 'views/assignContent/assignContent.html',
                controller: 'AssignContent'
            }
        }
    })
    .state('passList', {
        url: '/passList',
        controller:'PassList',
        templateUrl: '../views/passList/passList.html'
    })
    ;
    $urlRouterProvider.otherwise('/passList');
})