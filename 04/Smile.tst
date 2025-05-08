load Smile.asm,
output-file Smile.out,
compare-to Smile.cmp,
output-list RAM[16416]%D2.6.2 RAM[16448]%D2.6.2 RAM[16480]%D2.6.2 RAM[16512]%D2.6.2 RAM[16544]%D2.6.2 RAM[16576]%D2.6.2 RAM[16608]%D2.6.2 RAM[16640]%D2.6.2 RAM[20239]%D2.6.2 RAM[20271]%D2.6.2 RAM[20303]%D2.6.2 RAM[20335]%D2.6.2 RAM[20367]%D2.6.2 RAM[20399]%D2.6.2 RAM[20431]%D2.6.2 RAM[20463]%D2.6.2;

output;
repeat 1000 {
  ticktock;
}
output;

set PC 0,
set RAM[0] 3855;
repeat 1000 {
  ticktock;
}
output;
