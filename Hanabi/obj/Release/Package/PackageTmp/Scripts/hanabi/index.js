$(function () {
    console.log('yup');
    $.ajax({
        url: "/home/games",
        success: function (data) {
            console.log(data);
            for (var i = 0; i < data.length; i++) {
                var a = $('<a/>', {
                    href: '/home/game/' + data[i].game_id + '?user=' + user,
                    id: 'game' + data[i].game_id,
                    
                });
                var game = $('<div/>', {
                    class: 'game',
                    text: data[i].game_name
                });
                a.append(game);
                $('.games').append(a);
            }
        },
        error: function (data, status) {
            console.log('something went wrong, please try again' + data.responseText);
        }
    });

    $('#addgame').click(function () {
        var gamename = $('#game_name').val();
        if (gamename == "") {
            alert("please enter a valid game name");
            return;
        }
        var numplayers = parseInt($('#num_players').val());
        if (isNaN(numplayers) || numplayers > 5 || numplayers < 2) {
            alert("please enter a valid number of players");
            return;
        }
        var formData = { game_name:gamename, num_players:numplayers};
        $.ajax({
            url: "/home/create",
            type: 'POST',
            data: formData,
            success: function (data) {
                location.reload();
            },
            error: function (data, status) {
                alert('error, please try again');
                console.log('something went wrong, please try again' + data.responseText);
            }
        });
    });
});