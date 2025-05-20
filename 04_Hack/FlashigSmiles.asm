// loop:
//    Читаем клавишу
//
//    Если клавиша нажата:
//        Очищаем предыдущий смайл
//        Рисуем смайл
//        Запоминаем позицию, чтобы потом удалить
//    Иначе:
//        Очищаем экран



(SMILE)
// Запишем код смайла в память
// Он будет располагаться по адресу smile, smile + 8

    // Покажем ассемблеру что идущие подряд 8 ячеек заняты
    @smile
    @smile1
    @smile2
    @smile3
    @smile4
    @smile5
    @smile6
    @smile7

    @smile
    D = A   // D = smile
    @R1
    M = D   // R1 = smile

    // Начало записи

    @7224
    D = A   // D = 7224
    @R1
    A = M   // A = smile
    M = D   // smile = 7224 записали первую строку
    // Увеличиваем R1 + 1 для записи следующей строки
    @R1
    D = M   // D = smile
    D = M + 1   // D = smile + 1
    M = D   // smile = smile + 1

    @7224
    D = A
    @R1
    A = M
    M = D
    @R1
    D = M
    D = M + 1
    M = D


    @7224
    D = A
    @R1
    A = M
    M = D
    @R1
    D = M
    D = M + 1
    M = D

    @0
    D = A
    @R1
    A = M
    M = D
    @R1
    D = M
    D = M + 1
    M = D

    @0
    D = A
    @R1
    A = M
    M = D
    @R1
    D = M
    D = M + 1
    M = D

    @24582
    D = A
    @R1
    A = M
    M = D
    @R1
    D = M
    D = M + 1
    M = D

    @14364
    D = A
    @R1
    A = M
    M = D
    @R1
    D = M
    D = M + 1
    M = D

    @4080
    D = A
    @R1
    A = M
    M = D
    @R1
    D = M
    D = M + 1
    M = D

(CONSTANTS)
// Заведем константы для мест отрисовки

    // LEFT = 3968 + 10 + SCREEN
    @3968
    D = A
    @left
    M = D
    @10
    D = A
    @left
    M = M + D
    @SCREEN
    D = A
    @left
    M = M + D

    // UP = 2624 + 16 + SCREEN
    @2624
    D = A
    @up
    M = D
    @16
    D = A
    @up
    M = M + D
    @SCREEN
    D = A
    @up
    M = M + D

    // RIGHT = 3968 + 21 + SCREEN
    @3968
    D = A
    @right
    M = D
    @21
    D = A
    @right
    M = M + D
    @SCREEN
    D = A
    @right
    M = M + D

    // DOWN = 5280 + 16 + SCREEN
    @5280
    D = A
    @down
    M = D
    @16
    D = A
    @down
    M = M + D
    @SCREEN
    D = A
    @down
    M = M + D


(READKEY)   // В зависимости от нажатой клавиши рисуем смайл

// LEFT
    @130    // Клавиша на клавиатуре
    D = A
    @KBD
    D = D - M
    @DRAWSMILELEFT
    D; JEQ

// UP
    @131    // Клавиша на клавиатуре
    D = A
    @KBD
    D = D - M
    @DRAWSMILEUP
    D; JEQ

// RIGHT
    @132    // Клавиша на клавиатуре
    D = A
    @KBD
    D = D - M
    @DRAWSMILERIGHT
    D; JEQ

// DOWN
    @133    // Клавиша на клавиатуре
    D = A
    @KBD
    D = D - M
    @DRAWSMILEDOWN
    D; JEQ

// Очищаем последний нарисованый смайл, если не нажата клавиша
    // Установим адресс возврата
    @READKEY
    D = A
    @R1
    M = D
    // Код ниже произведет очистку


(CLEARPREVIOUS)
// Очищаем последний нарисованный смайл
// Параметры:
// R1: Адресс возврата

    // Запишем адресс смайла в R0
    @previousPosition
    D = M
    @R0
    M = D

    @CLEARSMILE     // Очистим область
    0; JMP

// Эти четыре блока устанавливают позицию смайла
(DRAWSMILEDOWN)
    @down
    D = M
    @smilePosition
    M = D
    @DRAWING
    0; JMP

(DRAWSMILEUP)
    @up
    D = M
    @smilePosition
    M = D
    @DRAWING
    0; JMP

(DRAWSMILELEFT)
    @left
    D = M
    @smilePosition
    M = D
    @DRAWING
    0; JMP

(DRAWSMILERIGHT)
    @right
    D = M
    @smilePosition
    M = D
    @DRAWING
    0; JMP


(DRAWING)
// Если smilePosition != previousPosition, нужно очистить предыдущий и нарисовать новый

    // Запишем адресс возврата в R1
    @DRAWINGCONTINUE
    D = A
    @R1
    M = D

    // D = smilePosition - previousPosition
    @smilePosition
    D = M
    @previousPosition
    D = D - M

    @CLEARPREVIOUS  // Очисътим предыдущий смайл
    D; JNE

(DRAWINGCONTINUE)   // Рисуем смайл

    // Запишем адресс смайла в R0
    @smilePosition
    D = M
    @R0
    M = D

    // Запишем адресс смайла в previousPosition
    @previousPosition
    M = D

    // Запишем адресс возврата в R1
    @READKEY
    D = A
    @R1
    M = D

// Код ниже нарисует смайл

(DRAWSMILE)     // Код для отрисовки смайла в произвольном месте на экране
// Входные Параметры:
// R0: адресс ячейки в которой нужно начать рисовать смайл
// R1: адресс возврата

    // Повторяем 8 раз
    @8
    D = A
    @i
    M = D   // i = 8

    @smile
    D = A
    @R2
    M = D   // R2 = smile

    @DRAWWHILE
    D = A
    @R3
    M = D   // R3 = DRAWWHILE


(DRAWWHILE)     // Цикл для отрисовки смайла
    // i = i - 1
    @i
    M = M - 1

    // D = RAM[R2] или smile
    @R2
    A = M
    D = M

    // RAM[R0] = D
    @R0
    A = M
    M = D

    // R2 = R2 + 1 или smile = smile + 1
    @R2
    M = M + 1

    // R0 = R0 + 32
    @32
    D = A
    @R0
    M = M + D

    // Переход к условию
    @IF
    0; JMP


(IF)    // Условие используемое в двух циклах
// Входные параметры:
// R1: адрес возврата
// R3: адрес для продолжения цикла, если увловие верное
    @i
    D = M
    @R3
    A = M
    D; JGT  // i > 0, выполняем цикл

    // Возвращаемся по адресу из R1
    @R1
    A = M
    0; JMP


(CLEARSMILE)    // Очищает экран от смайла
// Входные параметры:
// R0: адресс ячейки в которой начинается очистка экрана
// R1: адресс возврата

    // Повторяем 8 раз
    @8
    D = A
    @i
    M = D   // i = 8

    @CLEARWHILE
    D = A
    @R3
    M = D   // R3 = CLEARWHILE


(CLEARWHILE)    // Цикл для очистки экрана от смайла
    // i = i - 1
    @i
    M = M - 1

    // RAM[R0] = 0
    @R0
    A = M
    M = 0

    // R0 = R0 + 32
    @32
    D = A
    @R0
    M = M + D

    // Переход к условию
    @IF
    0; JMP


(END)
    @END
    0; JMP