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
});