// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/03/a/PC.tst

load Clock.hdl,
output-file Clock.out,
compare-to Clock.cmp,
output-list time%S1.4.1 period%D1.4.1 reset%B2.1.2 ticks%D1.3.1 loop%B2.1.1;

set reset 1, set period 1, tick, tock, output;
set reset 0, 
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
set reset 1, set period 2, tick, tock, output;
set reset 0, 
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
set reset 1, set period 3, tick, tock, output;
set reset 0, 
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
set reset 1, set period 11, tick, tock, output;
set reset 0, 
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
set period 3, tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
tick, tock, output;
set period 4, tick, tock, output;
tick, tock, output;
set period 5, tick, tock, output;
set period 7, tick, tock, output;
tick, tock, output;
set period 5, tick, tock, output;
set reset 1, set period 5, tick, tock, output;
tick, tock, output;
set reset 1, set period 6, tick, tock, output;
set reset 1, set period 2, tick, tock, output;