var MoodApp = angular.module('MoodApp', []);

MoodApp.controller('MoodController', function ($scope) {

    $scope.message = "MoodMessage";

});

MoodApp.factory('MoodService', ['$http', function ($http) {

    var MoodService = {};
    MoodService.getMoods = function () {
        return $http.get('/Mood/GetMoods');
    };
    MoodService.addMood = function (data) {
        return $http.post('/Mood/AddMood', data).success(function (mood) {
            console.log(mood);
            return mood;
        })
            .error(function (error) {
                $scope.status = 'Unable to add mood: ' + error.message;
                console.log($scope.status);
            });
    }
    MoodService.deleteMood = function (data) {
        return $http.delete('/Mood/DeleteMood?moodId=' + data.MoodId).success(function (theGuid) {
            console.log('deleted mood ' + theGuid);
        })
            .error(function (error) {
                $scope.status = 'Unable to delete mood: ' + error.message;
                console.log($scope.status);
            });
    }
    MoodService.getMoodAverages = function() {
        return $http.get('/Mood/GetMoodAveragesByDayOfWeek');
    }
    return MoodService;

}]);

MoodApp.controller('MoodController', function ($scope, MoodService) {

    $scope.maxNoteLength = 2000;

    getMoods();
    getMoodAverages();

    function getMoods() {
        MoodService.getMoods()
            .success(function (moods) {
                $scope.moods = moods;
                console.log($scope.moods);
            })
            .error(function (error) {
                $scope.status = 'Unable to load mood data: ' + error.message;
                console.log($scope.status);
            });
    }

    function getMoodAverages() {
        MoodService.getMoodAverages()
            .success(function(moods) {
                $scope.moodAverages = moods;
                console.log($scope.moods);
            })
            .error(function(error) {
                $scope.status = 'Unable to load mood average data: ' + error.message;
            });
    }

    $scope.addMood = function () {
        console.log("posting data:");
        console.log($scope.newMood);
        MoodService.addMood($scope.newMood)
            .success(function (mood) {
                $scope.newMood = null;
                getMoods();
                drawChart();
            })
            .error(function (error) {
                $scope.status = 'Unable to add mood: ' + error.message;
                console.log($scope.status);
            });
    }

    $scope.deleteMood = function (idx) {
        console.log('deleting index ' + idx);
        var moodToDelete = $scope.moods[idx];
        MoodService.deleteMood(moodToDelete)
            .success(function(data) {
                getMoods();
                drawChart();
            })
            .error(function(error) {
                console.log('Unable to delete mood: ' + error.message);
            });
    }

    $scope.dateOf = function (utcDateStr) {
        return new Date(utcDateStr);
    }

    

});