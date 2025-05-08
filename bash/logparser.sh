#!/bin/bash

if [ "$#" -ne 2 ]; then
    echo "logparser <input_log_file> <output_keys_file>"
    echo ""
    echo "Скрипт анализирует логи в поиске ключей с неудачными попытками обновления лимита для запроса"
    echo "по пути /api/news, закончившегося с кодом 418 и сохраняет уникальные ключи в выходной файл."
    exit 1
fi

input_file=$1
output_file=$2
temp_file=$(mktemp)

# Записываем все идентифитакторы трассировок от неудачных запросов во временный файл
grep -E 'End request: 418,.*\[GET /api/news\]' "$input_file" |
awk '{print $4}' > "$temp_file"

grep -F '[LimitRepository] Failed increment for: key=' "$input_file" |
awk -v trace_file="$temp_file" '
    BEGIN { 
        # Загружаем все trace_id в ассоциативный массив
        while (getline < trace_file) traces[$1]=1 
    }

    # Если trace_id текущей строки есть в массиве
    $4 in traces { 
        # Разбиваем строку по "key=" и берём вторую часть
        split($0, parts, "key=")

        # Разбиваем ключ по точке и берём первый элемент
        split(parts[2], key, ".")
        print key[1]
    }
' | uniq > "$output_file"

rm "$temp_file"