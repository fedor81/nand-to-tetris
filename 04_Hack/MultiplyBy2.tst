load MultiplyBy2.hack,
output-file MultiplyBy2.out,
compare-to MultiplyBy2.cmp,
output-list RAM[100]%D2.6.2;

set RAM[100] 0,
repeat 20 {
  ticktock;
}
output;

set PC 0,
set RAM[100] 1,
repeat 20 {
  ticktock;
}
output;

set PC 0,
set RAM[100] 2,
repeat 20 {
  ticktock;
}
output;
set PC 0,
repeat 20 {
  ticktock;
}
output;
set PC 0,
repeat 20 {
  ticktock;
}
output;
set PC 0,
repeat 20 {
  ticktock;
}
output;

set PC 0,
set RAM[100] 42,
repeat 20 {
  ticktock;
}
output;