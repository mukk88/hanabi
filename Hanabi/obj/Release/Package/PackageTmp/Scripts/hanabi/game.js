console.log(gameID);

Array.prototype.allValuesSame = function () {
    for (var i = 1; i < this.length; i++) {
        if (this[i] !== this[0])
            return false;
    }
    return true;
}
$(function () {
    var user = $('#username').html();
    //relocate if username is null
    $('#username').hide();
    $('#lastmove').hide();

    var mapping = {};
    mapping['0'] = '/Content/hanabi/dark/club.png';
    mapping['1'] = '/Content/hanabi/dark/diamond.png';
    mapping['2'] = '/Content/hanabi/dark/heart.png';
    mapping['3'] = '/Content/hanabi/dark/spade.png';
    mapping['4'] = '/Content/hanabi/dark/joker.png';

    var mappinglight = {};
    mappinglight['0'] = '/Content/hanabi/light/club.png';
    mappinglight['1'] = '/Content/hanabi/light/diamond.png';
    mappinglight['2'] = '/Content/hanabi/light/heart.png';
    mappinglight['3'] = '/Content/hanabi/light/spade.png';
    mappinglight['4'] = '/Content/hanabi/light/joker.png';

    var mappingblue = {};
    mappingblue['0'] = '/Content/hanabi/blue/club.png';
    mappingblue['1'] = '/Content/hanabi/blue/diamond.png';
    mappingblue['2'] = '/Content/hanabi/blue/heart.png';
    mappingblue['3'] = '/Content/hanabi/blue/spade.png';
    mappingblue['4'] = '/Content/hanabi/blue/joker.png';

    $('.play').hide();
    $('.moves').hide();
    $('#dialog').hide();
    if (gameID != '') {

        var cardClickable= function (isself) {
            $('.self .card').click(function () {
                //if (!isself) {
                //    alert('please wait for your turn');
                //    return;
                //}
                $('.move').hide();
                $('#discard').show();
                $('#use').show();
                $('.self .card').removeClass('selected');
                $(this).addClass('selected');
            });

            $('.player .card').click(function () {
                //if (!isself) {
                //    alert('please wait for your turn');
                //    return;
                //}
                $('.move').hide();
                $('#clue').show();
                $('.self .card').removeClass('selected');
                $(this).toggleClass('selected');
                $(this).find('> img').toggleClass('hidden');
            });
        };

        var cards = function (cards, istable, isself) {
            var hand = $('<div/>', {
                class: 'playerhand',
            });
            for (var j = 0; j < cards.length; j++) {
                var card = $('<div/>', {
                    class: cards[j].numinfo ? 'card color' : 'card',
                    title: j,
                    text: isself ? cards[j].numinfo ? cards[j].num : cards[j].suitinfo ? '?' : '' : istable ? cards[j] : cards[j].num
                });
                var imgdark = $('<img />', {
                    class: 'suit',
                    title: istable ? j : cards[j].suit,
                    src: istable ? mapping[j] : mapping[cards[j].suit]
                });
                var imglight = $('<img />', {
                    class: 'suit hidden',
                    title: istable ? j : cards[j].suit,
                    src: istable ? mappinglight[j] : mappinglight[cards[j].suit]
                });
                var imgblue = $('<img />', {
                    class: 'suit blue',
                    title: istable ? j : cards[j].suit,
                    src: mappingblue[cards[j].suit]
                });
                if (cards[j].suitinfo) {
                    card.append(imgblue);
                }else if (!isself) {
                    card.append(imgdark);
                    card.append(imglight);
                }
                hand.append(card);
            }
            return hand;
        };

        var cardcontainer = function(containerclass, text, index){
            var table = $('<div/>', {
                class: containerclass,
                id: 'container' + index
            });
            var name = $('<div/>', {
                class: 'playername',
                text: text
            });
            
            table.append(name);
            return table;
        };

        var addToDom = function (containerclass, text, cardsholder, istable, isself, index) {
            var table = cardcontainer(containerclass, text, index);
            var hand = cards(cardsholder, istable, isself);
            table.append(hand);
            $('.hands-built').append(table);
        }

        var buildFromData = function (data) {
            console.log(data);
            if (!data) return;
            $('.hands-built').html('');
            $('.move').hide();
            //$('#gamename').html(data.game_name)
            addToDom('Table', 'Table', data.table, true, false, 0);
            for (var i = 0; i < data.players.length; i++) {
                if (data.users[i] != user) {
                    addToDom('player', data.users[i] ? data.users[i] : data.players[i].name, data.players[i].hand, false, false, i);
                } else {
                    addToDom('self', 'Me', data.players[i].hand, false, true, 0);
                }
            }
            addToDom('discard', 'Discards', data.discards, false, false, 0);
            $('#clues').html('Clues: ' + data.clues);
            $('#burns').html('Burns: ' + data.burns);
            $('#deck').html('Deck: ' + data.deck.length);

            var turn = (data.turn % data.num_players);
            cardClickable(data.users[turn] == user);
            $('#playernum').html((data.users[turn] == '' ? data.players[turn].name + '\'s' : data.users[turn] == user ? 'Your' : data.users[turn] + '\'s') + ' turn');

            if (parseInt($('#clues').html().split('Burns: ')[1]) <= 0) {
                alert('You lost!');
            }

            if (data.last_move != '') {
                $('#lastmove').html(data.last_move).show().delay(3500).fadeOut('slow');
            }

        }

        $.ajax({
            url: "/home/data/" + gameID,
            success: function (data) {
                buildFromData(data);
            },
            error: function (data, status) {
                console.log('something went wrong, please try again' + data.responseText);
            }
        });

        var cluePost = function (to, indexes, isnum, cardInfos) {
            var formData = { username: user, to: to, game_id: gameID, indexes:indexes, isnum:isnum };
            if (!checkIfOtherValidCards(isnum, to, cardInfos)) {
                alert("that is not a valid clue");
                return;
            }

            $.ajax({
                url: '/home/clue',
                type: 'POST',
                data: formData,
                success: function (data) {
                    //location.reload();
                },
                error: function (data, status) {
                    console.log(data + ' ' + status);
                }
            });
        };

        var useOrDiscard = function (use) {
            var url = use ? '/home/play' : '/home/discard';
            var numcards = $('.self .card.selected').length;
            if (numcards != 1) {
                alert('wrong number of cards selected');
                return;
            }
            var cardindex = $($('.self .card.selected')[0]).attr('title');
            var formData = { username: user, card: cardindex, game_id: gameID };

            $.ajax({
                url: url,
                type: 'POST',
                data: formData,
                success: function (data) {
                    //location.reload();
                },
                error: function (data, status) {
                    console.log(data + ' ' + status);
                }
            });
        }

        $('#use').click(function () {
            useOrDiscard(true);
        });

        $('#discard').click(function () {
            useOrDiscard(false);
        });

        var checkIfOtherValidCards = function (isnum, playerToHint, cardInfos) {
            var count = 0;
            //find number or suit of the card
            var toCompare = cardInfos[0];
            // check number of cards of that suit.
            if (isnum) {
                $('#container' + playerToHint + ' .card').each(function () {
                    if ($(this).text() == toCompare) {
                        count++;
                    }
                });
            } else {
                $('#container' + playerToHint+ ' .card img:visible').each(function () {
                    if ($(this).attr('title') == toCompare) {
                        count++;
                    }
                });
            }
            // check if tally
            if (count == cardInfos.length) {
                return true;
            } else {
                return false;
            }
        }

        $('#clue').click(function () {
            if (parseInt($('#clues').html().split('Clues: ')[1]) <= 0) {
                alert('there are no more clues to give');
                return;
            }

            if ($('.player .card.selected').length < 1) {
                alert('please select at least one card');
                return;
            }

            //check that they are giving valid clue not 1 when there are 2 1's

            var playerToHint = '';
            var suits = [];
            var nums = [];
            var multiple = false;
            $('.player .card.selected').each(function () {
                if (playerToHint == '') {
                    playerToHint = $(this).parent().parent().attr('id').split('container')[1];
                } else {
                    if (playerToHint != $(this).parent().parent().attr('id').split('container')[1]) {
                        multiple = true;
                    }
                }
                var num = $(this).text();
                var suit = $(this).find('img').attr('title');
                suits.push(suit);
                nums.push(num);

            });
            if (multiple) {
                alert('can only hint to one person');
                return;
            }
            var indexes = '';
            $('.player .card.selected').each(function(){
                indexes += parseInt($(this).attr('title')) + ',';
            });
            indexes = indexes.substring(0, indexes.length - 1)
            if (nums.allValuesSame() && suits.allValuesSame()) {
                $('#dialog').dialog({
                    modal: true,
                    buttons: {
                        'Suit': function () {
                            cluePost(playerToHint, indexes, false, suits);
                            $(this).dialog('close');
                        },
                        'Number': function () {
                            cluePost(playerToHint, indexes, true, nums);
                            $(this).dialog('close');
                        }
                    }
                });
                return;
            }

            if (nums.allValuesSame()) {
                cluePost(playerToHint, indexes,true, nums);
                return;
            }
            if (suits.allValuesSame()) {
                cluePost(playerToHint, indexes, false, suits);
                return;
            }
            alert('cards have to be either of the same suit or same number');
        });

    } else {
        alert('the game is full or something went wrong, sorry');
    }

    $('#backbutton').click(function () {
        window.location = '/home';
    })

    var connection = $.hubConnection("http://mukkhanabi.azurewebsites.net");
    var dataHubProxy = connection.createHubProxy('hanabihub');
    // Create a function that the hub can call to broadcast messages.
    dataHubProxy.on('update', function (gamedata) {
        var data = $.parseJSON(gamedata);
        buildFromData(data);
    });
    connection.start().done(function () {
        dataHubProxy.invoke('joingame', gameID);
    });

});
