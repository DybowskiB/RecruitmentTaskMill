import { useEffect, useState } from "react";
import { createRequest, getStatus } from "./api";
import "./App.css";

const STORAGE_KEY = "requestId";

function App() {
  const [requestId, setRequestId] = useState(localStorage.getItem(STORAGE_KEY));
  const [status, setStatus] = useState("Idle");
  const [data, setData] = useState(null);
  const [error, setError] = useState("");

  const mapStatusToText = (state) => {
    switch (Number(state)) {
      case 0:
        return "Pending";
      case 1:
        return "Processing";
      case 2:
        return "Completed";
      case 3:
        return "Failed";
      default:
        return "Unknown";
    }
  };

  const isCompletedState = (state) => Number(state) === 2;
  const isFailedState = (state) => Number(state) === 3;
  const isProcessingState = (state) => Number(state) === 0 || Number(state) === 1;

  const startRequest = async () => {
    try {
      setError("");
      setData(null);
      setStatus("Pending");

      const result = await createRequest();

      localStorage.setItem(STORAGE_KEY, result.requestId);
      setRequestId(result.requestId);
    } catch (e) {
      setError(e.message || "Unexpected error");
      setStatus("Failed");
    }
  };

  useEffect(() => {
    if (!requestId) return;

    let isCancelled = false;
    let intervalId;

    const poll = async () => {
      try {
        const res = await getStatus(requestId);

        if (isCancelled) return;

        setStatus(mapStatusToText(res.state));
        setData(res.payload || null);
        setError(res.errorMessage || "");

        if (isCompletedState(res.state) || isFailedState(res.state)) {
          clearInterval(intervalId);
        }
      } catch (e) {
        if (isCancelled) return;

        setError(e.message || "Unexpected polling error");
        clearInterval(intervalId);
      }
    };

    poll();
    intervalId = setInterval(poll, 2000);

    return () => {
      isCancelled = true;
      clearInterval(intervalId);
    };
  }, [requestId]);

  const clear = () => {
    localStorage.removeItem(STORAGE_KEY);
    setRequestId(null);
    setStatus("Idle");
    setData(null);
    setError("");
  };

  return (
    <div className="container">
      <h1 className="title">Frontend</h1>

      <div className="buttons">
        <button onClick={startRequest}>Generate data</button>
        <button onClick={clear}>Clear</button>
      </div>

      <div className="info">
        <p><span className="label">RequestId:</span> {requestId || "-"}</p>
        <p><span className="label">Status:</span> {status}</p>
      </div>

      {isProcessingState(status) && (
        <p className="processing">Processing...</p>
      )}

      {error && <p className="error">Error: {error}</p>}

      {data && (
        <div className="data">
          <p><span className="label">ClientId:</span> {data.clientId}</p>
          <p><span className="label">Message:</span> {data.message}</p>
          <p><span className="label">Number:</span> {data.generatedNumber}</p>
          <p><span className="label">GeneratedAtUtc:</span> {data.generatedAtUtc}</p>
          <p><span className="label">CachedUntilUtc:</span> {data.cachedUntilUtc}</p>
        </div>
      )}
    </div>
  );
}

export default App;