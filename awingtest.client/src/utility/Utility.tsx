export class Utility {
    static async post<RequestObject>(url: string, body: RequestObject): Promise<RequestObject> {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(body),
    });

    return await response.json();
}
}
