#!/bin/bash

if [ "$#" -ne 2 ]; then
    echo "wikisearcher <Start Page> <Target Page>"
    echo ""
    echo "Скрипт проверяет возможность дойти от страницы https://en.wikipedia.org/wiki/<Start Page>"
    echo "до страницы https://en.wikipedia.org/wiki/<Target Page> по ссылкам из блока навигации."
    exit 1
fi

PAGE="$1"
TARGET_PAGE="$2"
BASE_URL="https://en.wikipedia.org/wiki/"

# Функция для извлечения ссылок навигации со страницы
extract_nav_links() {
    local page="$1"

    # Скачиваем страницу и извлекаем все td с классом navbox-list-with-group
    # Затем извлекаем все ссылки внутри этих td
    curl -s "$BASE_URL$page" |
        awk '' |
        sort -u
}

found=0
extract_nav_links 'https://en.wikipedia.org/wiki/Operating_system'

# Вывод
if [ "$found" -eq 1 ]; then
    echo "FOUND!"
else
    echo "NO WAY"
fi
