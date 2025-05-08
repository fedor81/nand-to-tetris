/**
 * The ALU (Arithmetic Logic Unit).
 * Computes one of the following functions:
 * x+y, x-y, y-x, 0, 1, -1, x, y, -x, -y, !x, !y,
 * x+1, y+1, x-1, y-1, x&y, x|y on two 16-bit inputs, 
 * according to 6 input bits denoted zx,nx,zy,ny,f,no.
 * In addition, the ALU computes two 1-bit outputs:
 * if the ALU output == 0, zr is set to 1; otherwise zr is set to 0;
 * if the ALU output < 0, ng is set to 1; otherwise ng is set to 0.
 */

// Implementation: the ALU logic manipulates the x and y inputs
// and operates on the resulting values, as follows:
// if (zx == 1) set x = 0        // 16-bit constant
// if (nx == 1) set x = !x       // bitwise not
// if (zy == 1) set y = 0        // 16-bit constant
// if (ny == 1) set y = !y       // bitwise not
// if (f == 1)  set out = x + y  // integer 2`s complement addition
// if (f == 0)  set out = x & y  // bitwise and
// if (no == 1) set out = !out   // bitwise not
// if (out == 0) set zr = 1
// if (out < 0) set ng = 1

CHIP ALU {
	IN  
		x[16], y[16],  // 16-bit inputs        
		zx, // zero the x input?
		nx, // negate the x input?
		zy, // zero the y input?
		ny, // negate the y input?
		f,  // compute out = x + y (if 1) or x & y (if 0)
		no; // negate the out output?

	OUT 
		out[16], // 16-bit output
		zr, // 1 if (out == 0), 0 otherwise
		ng; // 1 if (out < 0),  0 otherwise

	PARTS:
	// Обрабатываем zx
	Not(in=zx, out=notZX);
	Amplifier(in=notZX, out=notZX16);
	And16(a=x, b=notZX16, out=zeroX);

	// Определяем инвертировать ли x
	Not16(in=zeroX, out=notZeroX);
	Mux16(a=zeroX, b=notZeroX, sel=nx, out=newX);

	// Обрабатываем zy
	Not(in=zy, out=notZY);
	Amplifier(in=notZY, out=notZY16);
	And16(a=y, b=notZY16, out=zeroY);

	// Определяем инвертировать ли y
	Not16(in=zeroY, out=notZeroY);
	Mux16(a=zeroY, b=notZeroY, sel=ny, out=newY);

	// Выборка функции
	Add16(a=newX, b=newY, out=xAddY);
	And16(a=newX, b=newY, out=xAndY);
	Mux16(a=xAndY, b=xAddY, sel=f, out=newOut);

	// Определяем инвертировать ли out
	Not16(in=newOut, out=notOut);
	Mux16(a=newOut, b=notOut, sel=no, out=out, out[15]=ng, out=tempOut);

	// Устанавливаем zr, если out == 0
	Or16Way(in=tempOut, out=tempZR);
	Not(in=tempZR, out=zr);
}
