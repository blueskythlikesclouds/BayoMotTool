﻿using BayoMotTool;

var mapping = new Dictionary<int, int>
{
    {0, 0}, 
    {1, 1}, 
    {2, 2}, 
    {3, 3}, 
    {4, 4}, 
    {5, 6}, 
    {6, 7}, 
    {7, 9}, 
    {8, 10}, 
    {10, 13}, 
    {11, 15}, 
    {12, 16}, 
    {14, 19}, 
    {15, 20}, 
    {16, 21}, 
    {17, 22}, 
    {18, 23}, 
    {19, 24}, 
    {20, 25}, 
    {21, 26}, 
    {22, 27}, 
    {42, 107}, 
    {43, 108}, 
    {49, 28}, 
    {65, 5}, 
    {113, 8}, 
    {129, 29}, 
    {145, 11}, 
    {146, 30}, 
    {177, 14}, 
    {193, 31}, 
    {209, 17}, 
    {210, 32}, 
    {241, 33}, 
    {257, 34}, 
    {305, 35}, 
    {321, 36}, 
    {512, 45}, 
    {513, 64}, 
    {768, 78}, 
    {769, 79}, 
    {770, 80}, 
    {1024, 37}, 
    {1025, 38}, 
    {1026, 39}, 
    {1040, 40}, 
    {1041, 41}, 
    {1042, 42}, 
    {1043, 43}, 
    {1056, 44}, 
    {1057, 45}, 
    {1058, 46}, 
    {1059, 47}, 
    {1072, 48}, 
    {1073, 49}, 
    {1074, 50}, 
    {1076, 51}, 
    {1088, 52}, 
    {1089, 53}, 
    {1090, 54}, 
    {1091, 55}, 
    {1280, 56}, 
    {1281, 57}, 
    {1282, 58}, 
    {1296, 59}, 
    {1297, 60}, 
    {1298, 61}, 
    {1299, 62}, 
    {1312, 63}, 
    {1313, 64}, 
    {1314, 65}, 
    {1315, 66}, 
    {1328, 67}, 
    {1329, 68}, 
    {1330, 69}, 
    {1331, 70}, 
    {1344, 71}, 
    {1345, 72}, 
    {1346, 73}, 
    {1347, 74}, 
    {1536, 75}, 
    {1537, 76}, 
    {1538, 145}, 
    {1539, 146}, 
    {1540, 147}, 
    {1541, 148}, 
    {1542, 149}, 
    {1543, 299}, 

    // Bayonetta 3
    {2560, 8},
    {2561, 14},

    {2570, 33},
    {2571, 35},

    {2566, 30},
    {2567, 32},
};

var motion = new Motion();
motion.LoadBayo2(args[0]);

foreach (var record in motion.Records.Where(x => x.BoneIndex != 0xFFFF && !mapping.ContainsKey(x.BoneIndex)).DistinctBy(x => x.BoneIndex))
{
    Console.WriteLine(record.BoneIndex);
}

motion.Records.RemoveAll(x => x.BoneIndex != 0xFFFF && !mapping.ContainsKey(x.BoneIndex));

foreach (var record in motion.Records)
{
    if (record.BoneIndex != 0xFFFF)
        record.BoneIndex = (ushort)mapping[record.BoneIndex];
}

MotionUtility.AttachBone(motion, 8, 9);

MotionUtility.AttachBone(motion, 14, 15);

MotionUtility.SortRecords(motion);

motion.SaveBayo1(args.Length > 1 ? args[1] : args[0]);

