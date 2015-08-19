#include "stdafx.h"
#include "Launcher.h"
#include "Network.h"
#include "Shlwapi.h"
#include <CommCtrl.h>
#include <WinInet.h>
#include <regex>
#include <list>
#include <algorithm>
#include <sstream>
#include <iomanip> 
#include <winternl.h>
#include <stdio.h>
#include <atlstr.h>
#pragma comment (lib, "wininet.lib")
#pragma comment (lib, "Kernel32.lib")
using namespace std;

INT_PTR CALLBACK	DownloadDlg(HWND, UINT, WPARAM, LPARAM);
void CALLBACK DownloadProgress(_In_  HINTERNET hInternet, _In_  DWORD_PTR dwContext, _In_  DWORD dwInternetStatus, _In_  LPVOID lpvStatusInformation, _In_  DWORD dwStatusInformationLength);
VOID CALLBACK UpdateSpeed(_In_  HWND hwnd, _In_  UINT uMsg, _In_  UINT_PTR idEvent, _In_  DWORD dwTime);
string AutoScale(double amount, string unit);

typedef struct{
	HWND		hDialog;      // Window handle
	HINTERNET	hInternet;
	HINTERNET	hUrl;    // HINTERNET handle created by InternetOpenUrl
	wstring		filePath;
	UINT_PTR	timerID;
	HANDLE		finalization;
	char		buffer[512];
	FILE		*file;
	DWORD		size; // buffer fill size
	bool		complete;
	bool		cancel;
	void(*callback)(bool, wstring);
	unsigned int crc32;
	unsigned int lastCrc32;
	unsigned long long length; // file length
	unsigned long long received; // downloaded bytes so far
	unsigned long long lastReceived;
} DOWNLOAD_CONTEXT;

std::list<DOWNLOAD_CONTEXT> contexts;


void Download(TCHAR *url, wstring filePath, unsigned int crc32, unsigned long long length, void(*callback)(bool, wstring))
{
	DOWNLOAD_CONTEXT context;
	list<DOWNLOAD_CONTEXT>::iterator it;
	context.file = NULL;
	if (_wfopen_s(&context.file, filePath.c_str(), L"rb") == 0)
	{
		context.lastCrc32 = 0;
		while ((context.size = fread_s(&context.buffer, 512, 1, 512, context.file)) > 0)
			context.lastCrc32 = ComputeCrc32(context.lastCrc32, (BYTE*)&context.buffer, context.size);
		if (context.lastCrc32 == crc32)
		{
			(*callback)(true, filePath);
			fclose(context.file);
			return;
		}
	}
	if (context.file != NULL)
		fclose(context.file);
	context.hDialog = CreateDialog(hInst, MAKEINTRESOURCE(IDD_DIALOG1), hMainWindow, DownloadDlg);
	context.hInternet = InternetOpen(CA2T(userAgent.c_str()), INTERNET_OPEN_TYPE_PRECONFIG, NULL, NULL, INTERNET_FLAG_ASYNC);
	context.filePath = filePath;
	context.length = length;
	context.crc32 = crc32;
	context.finalization = CreateSemaphore(NULL, 1, 1, NULL);
	context.lastCrc32 = 0;
	context.received = 0;
	context.lastReceived = 0;
	context.size = 0;
	if (_wfopen_s(&context.file, filePath.c_str(), L"wb") != 0)
	{
		(*callback)(false, filePath);
		return;
	}
	context.callback = callback;
	context.complete = false;
	context.cancel = false;
	it = contexts.insert(contexts.begin(), context);
	InternetSetStatusCallback(context.hInternet, DownloadProgress);
	InternetOpenUrl(context.hInternet, url, _T(""), 0, INTERNET_FLAG_RELOAD, DWORD_PTR(&*it));
}

string DownloadToVar(TCHAR *url, bool followRedirect)
{
	string response("");
	HINTERNET hInternet = InternetOpen(CA2T(userAgent.c_str()), INTERNET_OPEN_TYPE_PRECONFIG, NULL, NULL, 0);
	HINTERNET hUrl = InternetOpenUrl(hInternet, url, _T(""), 0, INTERNET_FLAG_RELOAD | (followRedirect ? 0 : INTERNET_FLAG_NO_AUTO_REDIRECT), NULL);
	if (hInternet != 0 && hUrl != 0)
	{
		char buffer[512];
		DWORD numberOfBytesRead = 1;
		BOOL result = true;
		while (numberOfBytesRead != 0 && result != 0)
		{
			result = InternetReadFile(hUrl, buffer, 512, &numberOfBytesRead);
			response.append(buffer, numberOfBytesRead);
		}
	}
	InternetCloseHandle(hInternet);
	InternetCloseHandle(hUrl);
	return response;
}

void CALLBACK DownloadProgress(
	_In_  HINTERNET hInternet,
	_In_  DWORD_PTR dwContext,
	_In_  DWORD dwInternetStatus,
	_In_  LPVOID lpvStatusInformation,
	_In_  DWORD dwStatusInformationLength
	)
{
	DOWNLOAD_CONTEXT *context = (DOWNLOAD_CONTEXT*)dwContext;
	switch (dwInternetStatus)
	{
	case INTERNET_STATUS_COOKIE_SENT:
		break;

	case INTERNET_STATUS_COOKIE_RECEIVED:
		break;

	case INTERNET_STATUS_COOKIE_HISTORY:
		break;

	case INTERNET_STATUS_CLOSING_CONNECTION:
		break;

	case INTERNET_STATUS_CONNECTED_TO_SERVER:
		break;

	case INTERNET_STATUS_CONNECTING_TO_SERVER:
		break;

	case INTERNET_STATUS_CONNECTION_CLOSED:
		break;

		// not received on Windows XP
	case INTERNET_STATUS_HANDLE_CLOSING:
		break;

	case INTERNET_STATUS_HANDLE_CREATED:
		context->hUrl = (HINTERNET)((LPINTERNET_ASYNC_RESULT)lpvStatusInformation)->dwResult;
		ShowWindow(context->hDialog, SW_SHOW);
		context->timerID = SetTimer(context->hDialog, 0, 1000, UpdateSpeed);
		break;

	case INTERNET_STATUS_INTERMEDIATE_RESPONSE:
		break;

	case INTERNET_STATUS_RECEIVING_RESPONSE:
		break;

	case INTERNET_STATUS_RESPONSE_RECEIVED:
		break;

	case INTERNET_STATUS_REDIRECT:
		break;

	case INTERNET_STATUS_REQUEST_COMPLETE:
		if ((HINTERNET)((LPINTERNET_ASYNC_RESULT)lpvStatusInformation)->dwResult == context->hUrl)
		{
			DWORD size = 0;
			char *text;
			InternetQueryOptionA(context->hUrl, INTERNET_OPTION_URL, nullptr, &size);
			text = new char[size];
			InternetQueryOptionA(context->hUrl, INTERNET_OPTION_URL, text, &size);
			SetWindowTextA(GetDlgItem(context->hDialog, IDC_EDIT1), text);
			SetWindowTextW(GetDlgItem(context->hDialog, IDC_EDIT2), context->filePath.c_str());
			delete[] text;
		}
		if (((LPINTERNET_ASYNC_RESULT)lpvStatusInformation)->dwResult > 0 && !context->cancel)
		{
			DWORD res;
			do
			{
				context->received += context->size;
				fwrite(context->buffer, 1, context->size, context->file);
				res = InternetReadFile(context->hUrl, context->buffer, 512, &context->size);

			} while (res > 0 && context->size > 0 && !context->cancel);
			if (GetLastError() != ERROR_IO_PENDING || context->cancel)
			{
				context->complete = true;
				InternetCloseHandle(context->hUrl);
				InternetCloseHandle(hInternet);
			}
			std::stringstream ss;
			ss << AutoScale(context->received, "B out of ") << AutoScale(context->length, "B");
			SetWindowTextA(GetDlgItem(context->hDialog, IDC_STATIC_SIZE), ss.str().c_str());
			SendMessage(GetDlgItem(context->hDialog, IDC_PROGRESS1), PBM_SETPOS, context->received * 100 / context->length, 0);
		}
		else
		{
			context->complete = true;
			InternetCloseHandle(context->hUrl);
			InternetCloseHandle(hInternet);
		}
		if (context->complete && WaitForSingleObject(context->finalization, 0) == 0)
		{
			InternetSetStatusCallback(hInternet, NULL);
			KillTimer(context->hDialog, context->timerID);
			EndDialog(context->hDialog, NULL);
			// dangerous, so we let it leak .. it's a small amount anyway
			//contexts.remove_if([context](DOWNLOAD_CONTEXT it) -> bool {return it.hDialog == context->hDialog; });
			fclose(context->file);
			//check file
			context->file = NULL;
			if (_wfopen_s(&context->file, context->filePath.c_str(), L"rb") == 0)
			{
				context->lastCrc32 = 0;
				while ((context->size = fread_s(&context->buffer, 512, 1, 512, context->file)) > 0)
					context->lastCrc32 = ComputeCrc32(context->lastCrc32, (BYTE*)&context->buffer, context->size);
				fclose(context->file);
			}
			context->callback(context->lastCrc32 == context->crc32, context->filePath);
		}
		break;

	case INTERNET_STATUS_REQUEST_SENT:
		break;

	case INTERNET_STATUS_DETECTING_PROXY:
		break;

	case INTERNET_STATUS_RESOLVING_NAME:
		break;

	case INTERNET_STATUS_NAME_RESOLVED:
		break;

	case INTERNET_STATUS_SENDING_REQUEST:
		break;

	case INTERNET_STATUS_STATE_CHANGE:
		break;

	case INTERNET_STATUS_P3P_HEADER:
		break;

	default:
		//MessageBoxA(NULL, "Status: Unknown (%d)\n", "", MB_OK);
		break;
	}
}

// Message handler for dialog box.
INT_PTR CALLBACK DownloadDlg(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG:
		return (INT_PTR)TRUE;

	case WM_COMMAND:
		if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
		{
			EndDialog(hDlg, LOWORD(wParam));
			list<DOWNLOAD_CONTEXT>::iterator it;
			it = find_if(contexts.begin(), contexts.end(), [hDlg](DOWNLOAD_CONTEXT it) -> bool {return it.hDialog == hDlg; });
			if (it != contexts.end())
				it->cancel = true;
			return (INT_PTR)TRUE;
		}
		break;
	case WM_CLOSE:
		if (true)
		{
			list<DOWNLOAD_CONTEXT>::iterator it;
			it = find_if(contexts.begin(), contexts.end(), [hDlg](DOWNLOAD_CONTEXT it) -> bool {return it.hDialog == hDlg; });
			if (it != contexts.end())
				it->cancel = true;
		}
		break;
	}


	return (INT_PTR)FALSE;
}

VOID CALLBACK UpdateSpeed(
	_In_  HWND hwnd,
	_In_  UINT uMsg,
	_In_  UINT_PTR idEvent,
	_In_  DWORD dwTime
	)
{
	list<DOWNLOAD_CONTEXT>::iterator it;
	it = find_if(contexts.begin(), contexts.end(), [hwnd](DOWNLOAD_CONTEXT it) -> bool {return it.hDialog == hwnd; });
	if (it != contexts.end())
	{
		SetWindowTextA(GetDlgItem(hwnd, IDC_STATIC_SPEED), AutoScale(it->received - it->lastReceived, "B/s").c_str());
		it->lastReceived = it->received;
	}
}

//binary multiples - JEDEC
string AutoScale(double amount, string unit)
{
	stringstream ss;
	ss << std::fixed;
	if (amount >= 107374182400)
		ss << std::setprecision(0) << amount / 1024 / 1024 / 1024 << " G" << unit;
	else if (amount >= 10737418240)
		ss << std::setprecision(1) << amount / 1024 / 1024 / 1024 << " G" << unit;
	else if (amount >= 1073741824)
		ss << std::setprecision(2) << amount / 1024 / 1024 / 1024 << " G" << unit;
	else if (amount >= 104857600)
		ss << std::setprecision(0) << amount / 1024 / 1024 << " M" << unit;
	else if (amount >= 10485760)
		ss << std::setprecision(1) << amount / 1024 / 1024 << " M" << unit;
	else if (amount >= 1048576)
		ss << std::setprecision(2) << amount / 1024 / 1024 << " M" << unit;
	else if (amount >= 102400)
		ss << std::setprecision(0) << amount / 1024 << " K" << unit;
	else if (amount >= 10240)
		ss << std::setprecision(1) << amount / 1024 << " K" << unit;
	else if (amount >= 1024)
		ss << std::setprecision(2) << amount / 1024 << " K" << unit;
	else //if (amount < 1024)
		ss << std::setprecision(0) << amount << " " << unit;
	return ss.str();
}