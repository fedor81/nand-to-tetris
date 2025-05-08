(START)
    // Начальная позиция pos = SCREEN
    @SCREEN
    D = A
    @pos
    M = D

    // Конечная позиция
    @24576
    D = A
    @size
    M = D

    @i  // i = 0
    M = 0
    
(WHILE)
    // pos > screenSize конец программы
    @pos
    D = M
    @size
    D = D - M   // D = pos - screenSize
    @END
    D; JGT

(FOR)
    @256
    D = A
    @i
    D = D - M
    @DRAW_ON_POS   // i != 256
    D; JNE

    @pos
    M = M + 1
    @i
    M = 0

(DRAW_ON_POS)
    @pos    // RAM[pos] = 111111111111111
    A = M
    M = -1

(INCREMENT_POS)
    // pos += 2
    @2
    D = A
    @pos
    M = M + D

    @i  // i++
    M = M + 1

    @WHILE
    0; JMP


(END)
    @21000
    M = 0
(END2)
    @END2
    0; JMP