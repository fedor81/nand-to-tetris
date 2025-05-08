load TailRec.asm,
output-file TailRec.out,
compare-to TailRec.cmp,
output-list RAM[0]%D1.6.1 RAM[261]%D1.6.1;

repeat 2000000 {
  ticktock;
}

output;
