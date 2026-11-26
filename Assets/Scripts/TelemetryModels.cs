using System;

[Serializable]
public class NodeTelemetry
{
    public string name;
    public float cpu;
    public float mem;
    public int pods;
    public string status;
}

[Serializable]
public class ClusterTelemetry
{
    public string cluster;
    public string timestamp;
    public NodeTelemetry[] nodes; // JsonUtility needs a root object, not a bare array
}

[Serializable]
public class AuditReport
{
    public string userId;
    public float timeSpentSeconds;
    public int errors;
    public int score;
    public string timestamp;
}

[Serializable]
public class AuditResponse
{
    public string status;
    public string auditId;
    public string message;
}
