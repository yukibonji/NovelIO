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
open NovelFS.NovelIO.BinaryParser
open FsCheck
open FsCheck.Xunit

type ``Binary Pickler Combinator Tests`` =
    [<Property>]
    static member ``Unpickle tuple of ints from two ints`` (i1 : int, i2: int) =
        let bytes =
            Array.concat 
                [(System.BitConverter.GetBytes i1);
                (System.BitConverter.GetBytes i2)]
        let bytePickler = 
            BinaryPickler.tuple2 (BinaryPickler.pickleInt32) (BinaryPickler.pickleInt32)
        let result = BinaryPickler.unpickle bytePickler bytes 
        result = (i1, i2)

    [<Property>]
    static member ``Unpickle tuple of ints from three ints`` (i1 : int, i2: int, i3 : int) =
        let bytes =
            Array.concat 
                [(System.BitConverter.GetBytes i1);
                (System.BitConverter.GetBytes i2);
                (System.BitConverter.GetBytes i3)]
        let bytePickler = 
            BinaryPickler.tuple3 (BinaryPickler.pickleInt32) (BinaryPickler.pickleInt32) (BinaryPickler.pickleInt32)
        let result = BinaryPickler.unpickle bytePickler bytes 
        result = (i1, i2, i3)