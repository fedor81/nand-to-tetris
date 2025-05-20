load Formula.asm,
output-file Formula.out,
compare-to Formula.cmp,
output-list RAM[0]%D2.6.2 RAM[1]%D2.6.2 RAM[2]%D2.6.2 RAM[3]%D2.6.2 RAM[4]%D2.6.2 RAM[5]%D2.6.2;

set PC 0;
set RAM[0] 2;
set RAM[1] 3;
set RAM[2] 4;
set RAM[3] 5;
repeat 100 {
  ticktock;
}
output;

set PC 0;
set RAM[0] 523;
set RAM[1] 127;
set RAM[2] 419;
set RAM[3] 100;
repeat 100 {
  ticktock;
}
output;