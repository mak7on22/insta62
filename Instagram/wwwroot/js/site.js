$(document).ready(function () {
    let delayTimer;

    $("#searchInput").on("keyup", function () {
        clearTimeout(delayTimer);

        let searchTerm = $(this).val();
        console.log("Search term:", searchTerm);

        delayTimer = setTimeout(function () {
            $.ajax({
                url: '/User/Search/',
                data: { search: searchTerm },
                method: "GET",
                dataType: "text", 
                success: function (data) {
                    console.log("Search results:", data);
                    $("#searchResults").empty();

                    if (data.length > 0) {
                        let users = data.split(';');
                        $.each(users, function (index, user) {
                            let userInfo = user.split(',');
                            let userImg = userInfo[3];

                            let button = $("<a>")
                                .addClass("search")
                                .addClass("user-info");
                            let imgElement = $("<img>")
                                .attr("src", userImg)
                                .attr("width", "40")
                                .attr("height", "40");

                            // Создаем класс и стиль для текстового элемента
                            let textParts = [userInfo[1], userInfo[2], userInfo[4]];

                            let textElement = $("<span>")
                                .addClass("user-info-text")
                                .css("display", "flex"); // Добавляем стиль display: flex

                            // Создаем <p> элементы для каждой части текста и добавляем класс и стиль
                            textParts.forEach(function (part) {
                                textElement.append($("<p>").text(part).addClass("user-info-text-p").css("margin", "0"));
                            });

                            button.append(imgElement).append(textElement);

                            button.click(function () {
                                console.log("Пользователь:", userInfo);
                                window.location.href = '/User/Profile/' + userInfo[0];
                            });

                            $("#searchResults").append(button);

                            // Создаем классы для элементов userInfo[3], userInfo[1], userInfo[2], userInfo[4]
                            button.find("img").addClass("user-info-image");

                            // Применяем разные стили для каждого элемента
                        });

                        // Добавляем классы к каждому элементу .user-info
                        $(".user-info").each(function (index) {
                            $(this).addClass("user-" + index);
                        });

                        $(".user-info").css({
                            "border": "0.2px solid rgba(0,0,0,0.5)",
                            "border-radius": "5px",
                            "width": "290px",
                            "text-decoration": "auto",
                            "color": "black",
                            "font-weight": "700",
                            "padding": "5px",
                            "display": "flex",
                            "aling-items": "center",
                            "background-color": "rgba(162,160,160,0.8)"
                        });

                        // Добавляем стили для созданных классов
                        $(".user-info-image").css({
                            "border-radius": "50px"
                        });

                        // Добавляем стили для текстового элемента
                        $(".user-info-text").css({
                            "display": "flex",
                            "flex-direction": "column",
                            "line-height": "1",
                            "margin-left": "18px"
                            /* Другие стили, которые вы хотите применить */
                        });

                        // Добавляем стили для <p> элементов
                        $(".user-info-text-p").css({
                            "margin": "0",
                        });
                    }

                    else {
                        $("#searchResults").append("<p class='result' style='font-weight: bold'>Нет результатов.</p>");
                    }
                }
            });
        }, 300); 
    });
});
$(document).ready(function() {
    const photos = ["1.jpg", "2.jpg", "3.jpg"];
    let currentPhotoIndex = 0;

    function changePhoto() {
        const $currentPhons = $(".phons");
        const $nextPhons = $currentPhons.clone().attr("src", `/imgInst/${photos[currentPhotoIndex]}`);
        $nextPhons.css({ opacity: 0, zIndex: -1 });
        $currentPhons.after($nextPhons);
        $currentPhons.animate({ opacity: 0 }, 2000, function() {
            $currentPhons.remove();
            currentPhotoIndex = (currentPhotoIndex + 1) % photos.length;
        });
        $nextPhons.animate({ opacity: 1 }, 1000);
    }
    setInterval(changePhoto, 5000);
});
$(document).ready(function () {
    $("#fileInput").change(function () {
        let input = this;

        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $(".imagess").attr("src", e.target.result);
            };

            reader.readAsDataURL(input.files[0]); 
        }
    });
});















