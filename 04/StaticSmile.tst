load StaticSmile.asm,
output-file StaticSmile.out,
compare-to StaticSmile.cmp,
output-list RAM[16384]%D2.6.2 RAM[16416]%D2.6.2 RAM[16448]%D2.6.2 RAM[16480]%D2.6.2 RAM[16512]%D2.6.2 RAM[16544]%D2.6.2 RAM[16576]%D2.6.2 RAM[16608]%D2.6.2;

output;
repeat 1000 {
  ticktock;
}
output;