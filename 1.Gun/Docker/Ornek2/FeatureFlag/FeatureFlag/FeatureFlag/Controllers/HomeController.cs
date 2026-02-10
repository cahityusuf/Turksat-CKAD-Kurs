using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FeatureFlag.Models;
using Microsoft.FeatureManagement;

namespace FeatureFlag.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IFeatureManager _featureManager;

    public HomeController(ILogger<HomeController> logger, IFeatureManager featureManager)
    {
        _logger = logger; //test
        _featureManager = featureManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        var result = new FeatureManagementModel
        {
            FeatureB = "cem cem",
            FeatureA = "cahit kafadar"
        };
        return Json(result);

    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Trigger continuous logging for alert testing
    /// Generates logs for 1 minute with 5 second intervals
    /// </summary>
    [HttpPost("trigger-alert-test")]
    public async Task<IActionResult> TriggerAlertTest()
    {
        _logger.LogInformation("🚀 ALERT TEST STARTED - Will log for 1 minute");
        
        // Start background task for continuous logging
        _ = Task.Run(async () =>
        {
            var startTime = DateTime.UtcNow;
            var duration = TimeSpan.FromMinutes(1);
            var interval = TimeSpan.FromSeconds(5);
            var count = 0;
            
            while (DateTime.UtcNow - startTime < duration)
            {
                count++;
                // Log the exact pattern that Loki alert is watching for
                _logger.LogInformation("Hosting environment: Production");
                _logger.LogInformation($"Alert test log #{count} - Pattern: 'Hosting environment: Production'");
                
                await Task.Delay(interval);
            }
            
            _logger.LogInformation($"✅ ALERT TEST COMPLETED - Generated {count} logs in 1 minute");
        });
        
        return Ok(new 
        { 
            success = true, 
            message = "Alert test started - logging for 1 minute with 5 second intervals",
            pattern = "Hosting environment: Production",
            duration = "1 minute",
            interval = "5 seconds",
            estimatedLogs = 12
        });
    }

    /// <summary>
    /// Get simple HTML page to trigger alert test
    /// </summary>
    [HttpGet("alert-test")]
    public IActionResult AlertTestPage()
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>DevOpsZon Alert Test</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            padding: 20px;
        }
        .container {
            background: white;
            padding: 40px;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            max-width: 600px;
            width: 100%;
        }
        h1 {
            color: #667eea;
            margin: 0 0 20px 0;
            font-size: 28px;
        }
        .info {
            background: #f0f6fc;
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            border-left: 4px solid #667eea;
        }
        .info p {
            margin: 8px 0;
            color: #333;
        }
        .info strong {
            color: #667eea;
        }
        button {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            padding: 16px 32px;
            font-size: 16px;
            font-weight: bold;
            border-radius: 8px;
            cursor: pointer;
            width: 100%;
            transition: all 0.3s;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
        }
        button:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
        }
        button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }
        .status {
            margin-top: 20px;
            padding: 16px;
            border-radius: 8px;
            text-align: center;
            font-weight: bold;
        }
        .success {
            background: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        .loading {
            background: #fff3cd;
            color: #856404;
            border: 1px solid #ffeaa7;
        }
        .countdown {
            font-size: 24px;
            color: #667eea;
            font-weight: bold;
            margin: 10px 0;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🔔 DevOpsZon Alert Test</h1>
        <div class='info'>
            <p><strong>Test Pattern:</strong> ""Hosting environment: Production""</p>
            <p><strong>Duration:</strong> 1 minute</p>
            <p><strong>Interval:</strong> Every 5 seconds</p>
            <p><strong>Expected Logs:</strong> ~12 logs</p>
            <p><strong>Alert Condition:</strong> 1 log in 1 minute (threshold: 0.0167)</p>
        </div>
        <button id='triggerBtn' onclick='triggerTest()'>🚀 START ALERT TEST</button>
        <div id='status'></div>
    </div>

    <script>
        let countdownInterval;
        
        async function triggerTest() {
            const btn = document.getElementById('triggerBtn');
            const status = document.getElementById('status');
            
            btn.disabled = true;
            status.className = 'status loading';
            status.innerHTML = '<div>⏳ Generating logs...</div><div class=\""countdown\"" id=\""countdown\"">60</div>';
            
            try {
                const response = await fetch('/trigger-alert-test', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });
                
                const result = await response.json();
                
                if (result.success) {
                    // Start countdown
                    let seconds = 60;
                    const countdownEl = document.getElementById('countdown');
                    
                    countdownInterval = setInterval(() => {
                        seconds--;
                        if (countdownEl) countdownEl.textContent = seconds;
                        
                        if (seconds <= 0) {
                            clearInterval(countdownInterval);
                            status.className = 'status success';
                            status.innerHTML = '✅ Test completed! Check Teams channel for alert notification.';
                            btn.disabled = false;
                        }
                    }, 1000);
                } else {
                    status.className = 'status error';
                    status.textContent = '❌ Failed to start test';
                    btn.disabled = false;
                }
            } catch (error) {
                status.className = 'status error';
                status.textContent = '❌ Error: ' + error;
                btn.disabled = false;
            }
        }
    </script>
</body>
</html>";
        
        return Content(html, "text/html");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        
    }
}