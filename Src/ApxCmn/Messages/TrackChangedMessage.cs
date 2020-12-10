﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Runtime.Serialization;

namespace ApxCmn.Messages
{
    [DataContract]
    public class TrackChangedMessage
    {
        public TrackChangedMessage()
        {
        }

        public TrackChangedMessage(Uri trackId)
        {
            this.TrackId = trackId;
        }

        [DataMember]
        public Uri TrackId;
    }
}
