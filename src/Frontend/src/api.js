const BASE_URL = "https://localhost:7126";

export async function createRequest() {
  const res = await fetch(`${BASE_URL}/api/data/request`, {
    method: "POST",
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Failed to create request. Status: ${res.status}`);
  }

  return res.json();
}

export async function getStatus(requestId) {
  const res = await fetch(`${BASE_URL}/api/data/request/${requestId}`, {
    method: "GET",
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Failed to get status. Status: ${res.status}`);
  }

  return res.json();
}