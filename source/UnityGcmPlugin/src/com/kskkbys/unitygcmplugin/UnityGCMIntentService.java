package com.kskkbys.unitygcmplugin;

import java.util.Set;

import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.google.android.gcm.GCMBaseIntentService;

/**
 * GCMIntentService.<br>
 * For each callback, this class sends message to GameObject via UnitySendMessage.
 * @author Keisuke Kobayashi
 *
 */
public class UnityGCMIntentService extends GCMBaseIntentService {

	private static final String TAG = UnityGCMIntentService.class.getSimpleName();

	private static final String ON_ERROR = "OnError";
	private static final String ON_MESSAGE = "OnMessage";
	private static final String ON_REGISTERED = "OnRegistered";
	private static final String ON_UNREGISTERED = "OnUnregistered";
	
	private static final String ON_DELETE_MESSAGES = "OnDeleteMessages";

	@Override
	protected void onError(Context context, String errorId) {
		Log.v(TAG, "onError");
		Util.sendMessage(ON_ERROR, errorId);
	}

	@Override
	protected void onMessage(Context context, Intent intent) {
		Log.v(TAG, "onMessage");
		// Notify to C# layer
		Bundle bundle = intent.getExtras();
		Set<String> keys = bundle.keySet();
		JSONObject json = new JSONObject();
		JSONObject jsonData = null;
		try {
			for (String key : keys) {
				Log.v(TAG, key + ": " + bundle.get(key));
				json.put(key, bundle.get(key));
			}
			jsonData = new JSONObject(json.getString("data"));
			Util.sendMessage(ON_MESSAGE, jsonData.toString());
		} catch (JSONException e) {
			e.printStackTrace();
		}
		catch (Exception e) {
			e.printStackTrace();
		}
		
		if (!Util.notificationsEnabled) {
			return;
		}
		
		// Show native notification view in status bar if defined fields are put.

		String contentTitle;
		try {
			contentTitle = jsonData.getString("content_title");
		} catch (JSONException e) {
			contentTitle = getAppLable();
		}
		String contentText;
		try {
			contentText = jsonData.getString("alert");
		} catch (JSONException e) {
			contentText = "";
		}
		String ticker;
		try {
			ticker = jsonData.getString("ticker");
		} catch (JSONException e) {
			ticker = contentTitle; // If no ticker specified, use title
		}
		UnityGCMNotificationManager.showNotification(this, contentTitle, contentText, ticker);

	}

	//http://stackoverflow.com/questions/11229219/android-get-application-name-not-package-name
	private String getAppLable() {
		//return getString(getApplicationInfo().labelRes);
		android.content.pm.PackageManager lPackageManager = getPackageManager();
	    	android.content.pm.ApplicationInfo lApplicationInfo = null;
	    	try {
	        	lApplicationInfo = lPackageManager.getApplicationInfo(getApplicationInfo().packageName, 0);
	    	} catch (Exception e) {
	    	}
	    	return (String) (lApplicationInfo != null ? lPackageManager.getApplicationLabel(lApplicationInfo) : "");
	}
	
	
	@Override
	protected void onRegistered(Context context, String registrationId) {
		Log.v(TAG, "onRegistered");
		Util.sendMessage(ON_REGISTERED, registrationId);
	}

	@Override
	protected void onUnregistered(Context context, String registrationId) {
		Log.v(TAG, "onUnregistered");
		Util.sendMessage(ON_UNREGISTERED, registrationId);
	}
	
	@Override
	protected void onDeletedMessages (Context context, int total) {
		Log.v(TAG, "onDeleteMessages");
		Util.sendMessage(ON_DELETE_MESSAGES, Integer.toString(total));
	}

}
