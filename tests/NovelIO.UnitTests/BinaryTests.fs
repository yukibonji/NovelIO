﻿(*
   Copyright 2015 Philip Curzon

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*)

namespace NovelFS.NovelIO.UnitTests

open NovelFS.NovelIO
open NovelFS.NovelIO.BinaryPickler
open FsCheck
open FsCheck.Xunit

type ``Binary Pickler Tests`` =
    [<Property>]
    static member ``Unpickle byte from array of one byte`` (byte : byte) =
        let bytes = [|byte|]
        let bytePickler = BinaryPickler.bytePU
        let result = BinaryPickler.unpickle bytePickler bytes 
        result = byte

    [<Property>]
    static member ``Unpickle int16 from array of bytes`` (i16 : int16) =
        let bytes = System.BitConverter.GetBytes i16
        let int16Pickler = BinaryPickler.int16PU
        let result = BinaryPickler.unpickle int16Pickler bytes
        result = i16

    [<Property>]
    static member ``Unpickle int from array of bytes`` (i32 : int32) =
        let bytes = System.BitConverter.GetBytes i32
        let int32Pickler = BinaryPickler.intPU
        let result = BinaryPickler.unpickle int32Pickler bytes
        result = i32

    [<Property>]
    static member ``Unpickle int64 from array of bytes`` (i64 : int64) =
        let bytes = System.BitConverter.GetBytes i64
        let int64Pickler = BinaryPickler.int64PU
        let result = BinaryPickler.unpickle int64Pickler bytes
        result = i64

    [<Property>]
    static member ``Unpickle float64 from array of bytes`` (flt : float) =
        let bytes = System.BitConverter.GetBytes flt
        let floatPickler = BinaryPickler.floatPU
        let result = BinaryPickler.unpickle floatPickler bytes
        match result with
        |x when System.Double.IsNaN(x) -> System.Double.IsNaN(flt)
        |_ -> result = flt

    [<Property>]
    static member ``Unpickle float32 from array of bytes`` (flt : float32) =
        let bytes = System.BitConverter.GetBytes flt
        let float32Pickler = BinaryPickler.float32PU
        let result = BinaryPickler.unpickle float32Pickler bytes
        match result with
        |x when System.Single.IsNaN(x) -> System.Single.IsNaN(flt)
        |_ -> result = flt

    [<Property>]
    static member ``Unpickle decimal from array of bytes`` (dec : decimal) =
        let bytes = 
            System.Decimal.GetBits dec
            |> Array.collect (System.BitConverter.GetBytes)
        let decPickler = BinaryPickler.decimalPU
        let result = BinaryPickler.unpickle decPickler bytes
        result = dec

    [<Property>]
    static member ``Unpickle Ascii string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.ASCII.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let stringPickler = BinaryPickler.asciiPU
        let result = BinaryPickler.unpickle stringPickler bytes
        result = str

    [<Property>]
    static member ``Unpickle UTF-7 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.UTF7.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let stringPickler = BinaryPickler.utf7PU
        let result = BinaryPickler.unpickle stringPickler bytes
        result = str

    [<Property>]
    static member ``Unpickle UTF-8 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.UTF8.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let stringPickler = BinaryPickler.utf8PU
        let result = BinaryPickler.unpickle stringPickler bytes
        result = str

    [<Property>]
    static member ``Unpickle Little Endian UTF-16 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let preamble = System.Text.Encoding.Unicode.GetPreamble()
        let bytesWOPrefix = System.Text.Encoding.Unicode.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let stringPickler = BinaryPickler.utf16PULtE
        let result = BinaryPickler.unpickle stringPickler bytes
        result = str

    [<Property>]
    static member ``Unpickle Big Endian UTF-16 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let preamble = System.Text.Encoding.BigEndianUnicode.GetPreamble()
        let bytesWOPrefix = System.Text.Encoding.BigEndianUnicode.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let stringPickler = BinaryPickler.utf16PUBgE
        let result = BinaryPickler.unpickle stringPickler bytes
        result = str

    [<Property>]
    static member ``Unpickle Little Endian UTF-32 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.UTF32.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let stringPickler = BinaryPickler.utf32PULtE
        let result = BinaryPickler.unpickle stringPickler bytes
        result = str

    [<Property>]
    static member ``Unpickle Big Endian UTF-32 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.UTF32Encoding(true, true).GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let stringPickler = BinaryPickler.utf32PUBgE
        let result = BinaryPickler.unpickle stringPickler bytes
        result = str

    [<Property>]
    static member ``Pickle byte from one byte`` (byte : byte) =
        let bytes = [|byte|]
        let bytePickler = BinaryPickler.bytePU
        let result = BinaryPickler.pickle bytePickler byte 
        result = bytes

    [<Property>]
    static member ``Pickle int16 from one int16`` (i16 : int16) =
        let int16Pickler = BinaryPickler.int16PU
        let bytes = BinaryPickler.pickle int16Pickler i16
        let result = BinaryPickler.unpickle int16Pickler bytes
        result = i16

    [<Property>]
    static member ``Pickle int from one int`` (i32 : int32) =
        let int32Pickler = BinaryPickler.intPU
        let bytes = BinaryPickler.pickle int32Pickler i32
        let result = BinaryPickler.unpickle int32Pickler bytes
        result = i32

    [<Property>]
    static member ``Pickle int64 from one int64`` (i64 : int64) =
        let int64Pickler = BinaryPickler.int64PU
        let bytes = BinaryPickler.pickle int64Pickler i64
        let result = BinaryPickler.unpickle int64Pickler bytes
        result = i64

    [<Property>]
    static member ``Pickle float from one float`` (f64 : float) =
        let floatPickler = BinaryPickler.floatPU
        let bytes = BinaryPickler.pickle floatPickler f64
        let result = BinaryPickler.unpickle floatPickler bytes
        match result with
        |x when System.Double.IsNaN(x) -> System.Double.IsNaN(f64)
        |_ -> result = f64

    [<Property>]
    static member ``Pickle float32 from one float32`` (f32 : float32) =
        let floatPickler = BinaryPickler.float32PU
        let bytes = BinaryPickler.pickle floatPickler f32
        let result = BinaryPickler.unpickle floatPickler bytes
        match result with
        |x when System.Single.IsNaN(x) -> System.Single.IsNaN(f32)
        |_ -> result = f32

    [<Property>]
    static member ``Pickle decimal from one decimal`` (dec : decimal) =
        let decPickler = BinaryPickler.decimalPU
        let bytes = BinaryPickler.pickle decPickler dec
        let result = BinaryPickler.unpickle decPickler bytes
        result = dec

    [<Property>]
    static member ``Pickle Ascii from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.asciiPU
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle UTF-7 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf7PU
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle UTF-8 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf8PU
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle UTF-8 with byte order mark`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf8PUBom
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle Little Endian UTF-16 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf16PULtE
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle Big Endian UTF-16 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf16PUBgE
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle UTF-16 with Endianness detection from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf16PU
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle Little Endian UTF-32 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf32PULtE
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle Big Endian UTF-32 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf32PUBgE
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

    [<Property>]
    static member ``Pickle UTF-32 with Endianness detection from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let strPickler = BinaryPickler.utf32PU
        let bytes = BinaryPickler.pickle strPickler str
        let result = BinaryPickler.unpickle strPickler bytes
        result = str

type ``Incremental Binary Pickler Tests`` =
    [<Property>]
    static member ``Unpickle byte from array of one byte`` (byte : byte) =
        let bytes = [|byte|]
        let buff = MemoryBuffer.createFromByteArray bytes
        let bytePickler = BinaryPickler.bytePU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr bytePickler bHandle
        } |> IO.run = byte

    [<Property>]
    static member ``Unpickle int16 from array of bytes`` (i16 : int16) =
        let bytes = System.BitConverter.GetBytes i16
        let buff = MemoryBuffer.createFromByteArray bytes
        let int16Pickler = BinaryPickler.int16PU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr int16Pickler bHandle
        } |> IO.run = i16

    [<Property>]
    static member ``Unpickle int from array of bytes`` (i32 : int32) =
        let bytes = System.BitConverter.GetBytes i32
        let buff = MemoryBuffer.createFromByteArray bytes
        let int32Pickler = BinaryPickler.intPU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr int32Pickler bHandle
        } |> IO.run = i32

    [<Property>]
    static member ``Unpickle int64 from array of bytes`` (i64 : int64) =
        let bytes = System.BitConverter.GetBytes i64
        let buff = MemoryBuffer.createFromByteArray bytes
        let int32Pickler = BinaryPickler.int64PU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr int32Pickler bHandle
        } |> IO.run = i64
        
    [<Property>]
    static member ``Unpickle float32 from array of bytes`` (f32 : float32) =
        let bytes = System.BitConverter.GetBytes f32
        let buff = MemoryBuffer.createFromByteArray bytes
        let float32Pickler = BinaryPickler.float32PU
        let result = 
            io {
                let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
                return! BinaryPickler.unpickleIncr float32Pickler bHandle
            } |> IO.run 
        match result with
        |x when System.Single.IsNaN(x) -> System.Single.IsNaN(f32)
        |_ -> result = f32

    [<Property>]
    static member ``Unpickle float from array of bytes`` (f64 : float) =
        let bytes = System.BitConverter.GetBytes f64
        let buff = MemoryBuffer.createFromByteArray bytes
        let float64Pickler = BinaryPickler.floatPU
        let result = 
            io {
                let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
                return! BinaryPickler.unpickleIncr float64Pickler bHandle
            } |> IO.run
        match result with
        |x when System.Double.IsNaN(x) -> System.Double.IsNaN(f64)
        |_ -> result = f64

    [<Property>]
    static member ``Unpickle decimal from array of bytes`` (dec : decimal) =
        let bytes = 
            System.Decimal.GetBits dec
            |> Array.collect (System.BitConverter.GetBytes)
        let buff = MemoryBuffer.createFromByteArray bytes
        let decPickler = BinaryPickler.decimalPU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr decPickler bHandle
        } |> IO.run = dec

    [<Property>]
    static member ``Unpickle Ascii string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.ASCII.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let buff = MemoryBuffer.createFromByteArray bytes
        let stringPickler = BinaryPickler.asciiPU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr stringPickler bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Unpickle UTF-7 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.UTF7.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let buff = MemoryBuffer.createFromByteArray bytes
        let stringPickler = BinaryPickler.utf7PU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr stringPickler bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Unpickle UTF-8 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.UTF8.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let buff = MemoryBuffer.createFromByteArray bytes
        let stringPickler = BinaryPickler.utf8PU
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr stringPickler bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Unpickle Little Endian UTF-16 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let preamble = System.Text.Encoding.Unicode.GetPreamble()
        let bytesWOPrefix = System.Text.Encoding.Unicode.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let buff = MemoryBuffer.createFromByteArray bytes
        let stringPickler = BinaryPickler.utf16PULtE
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr stringPickler bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Unpickle Big Endian UTF-16 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let preamble = System.Text.Encoding.BigEndianUnicode.GetPreamble()
        let bytesWOPrefix = System.Text.Encoding.BigEndianUnicode.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let buff = MemoryBuffer.createFromByteArray bytes
        let stringPickler = BinaryPickler.utf16PUBgE
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr stringPickler bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Unpickle Little Endian UTF-32 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.Encoding.UTF32.GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let buff = MemoryBuffer.createFromByteArray bytes
        let stringPickler = BinaryPickler.utf32PULtE
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr stringPickler bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Unpickle Big Endian UTF-32 string from array of bytes`` (nStr : NonEmptyString) =
        let str = nStr.Get 
        let bytesWOPrefix = System.Text.UTF32Encoding(true, true).GetBytes str
        let bytes = 
            Array.concat 
                [System.BitConverter.GetBytes (Array.length bytesWOPrefix);
                 bytesWOPrefix]
        let buff = MemoryBuffer.createFromByteArray bytes
        let stringPickler = BinaryPickler.utf32PUBgE
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            return! BinaryPickler.unpickleIncr stringPickler bHandle
        } |> IO.run = str


    [<Property>]
    static member ``Pickle byte from one byte`` (byte : byte) =
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.bytePU) bHandle byte
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.bytePU) bHandle
        } |> IO.run = byte

    [<Property>]
    static member ``Pickle int16 from one int16`` (i16 : int16) =
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.int16PU) bHandle i16
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.int16PU) bHandle
        } |> IO.run = i16

    [<Property>]
    static member ``Pickle int from one int`` (i32 : int32) =
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.intPU) bHandle i32
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.intPU) bHandle
        } |> IO.run = i32

    [<Property>]
    static member ``Pickle int64 from one int64`` (i64 : int64) =
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.int64PU) bHandle i64
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.int64PU) bHandle
        } |> IO.run = i64

    [<Property>]
    static member ``Pickle float from one float`` (f64 : float) =
        let buff = MemoryBuffer.createExpandable()
        let result = 
            io {
                let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
                do! BinaryPickler.pickleIncr (BinaryPickler.floatPU) bHandle f64
                do! IO.bhSetAbsPosition bHandle 0L
                return! BinaryPickler.unpickleIncr (BinaryPickler.floatPU) bHandle
            } |> IO.run
        match result with
        |x when System.Double.IsNaN(x) -> System.Double.IsNaN(f64)
        |_ -> result = f64

    [<Property>]
    static member ``Pickle float32 from one float32`` (f32 : float32) =
        let buff = MemoryBuffer.createExpandable()
        let result = 
            io {
                let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
                do! BinaryPickler.pickleIncr (BinaryPickler.float32PU) bHandle f32
                do! IO.bhSetAbsPosition bHandle 0L
                return! BinaryPickler.unpickleIncr (BinaryPickler.float32PU) bHandle
            } |> IO.run
        match result with
        |x when System.Single.IsNaN(x) -> System.Single.IsNaN(f32)
        |_ -> result = f32

    [<Property>]
    static member ``Pickle decimal from one decimal`` (dec : decimal) =
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.decimalPU) bHandle dec
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.decimalPU) bHandle
        } |> IO.run = dec

    [<Property>]
    static member ``Pickle Ascii from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.asciiPU) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.asciiPU) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle UTF-7 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf7PU) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf7PU) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle UTF-8 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf8PU) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf8PU) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle UTF-8 with byte order mark`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf8PUBom) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf8PUBom) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle Little Endian UTF-16 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf16PULtE) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf16PULtE) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle Big Endian UTF-16 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf16PUBgE) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf16PUBgE) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle UTF-16 with Endianness detection from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf16PU) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf16PU) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle Little Endian UTF-32 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf32PULtE) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf32PULtE) bHandle
        } |> IO.run = str

    [<Property>]
    static member ``Pickle Big Endian UTF-32 from string`` (nStr : NonEmptyString) =
        let str = nStr.Get
        let buff = MemoryBuffer.createExpandable()
        io {
            let! bHandle = MemoryBuffer.bufferToBinaryHandle buff
            do! BinaryPickler.pickleIncr (BinaryPickler.utf32PUBgE) bHandle str
            do! IO.bhSetAbsPosition bHandle 0L
            return! BinaryPickler.unpickleIncr (BinaryPickler.utf32PUBgE) bHandle
        } |> IO.run = str



    
        


