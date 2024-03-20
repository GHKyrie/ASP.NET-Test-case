using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Test_Case.Models;
using Newtonsoft.Json;

namespace Test_Case.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public JsonResult Calculate(string amount, string term, string interest)
        {
            #region "Объявление"
            double loanAmount = Convert.ToDouble(amount);                                           //  S - сумма платежа кредита
            double loanInterest = Convert.ToDouble(interest) / 100;                                 //  P - ставка (в год, %)
            int loanTerm = Convert.ToInt32(term);                                                   //  n - срок займа (в месяцах)

            List<double> percents = new List<double>();                                             //  Платежи по %
            List<double> bodyDebts = new List<double>();                                            //  Платежи по телу
            List<double> remainders = new List<double>();                                           //  Оставшийся долги по периодам
            #endregion

            if (loanAmount == 0)
            {
                return null;
            }

            #region "Месячный платеж"
            //  i - месячный %
            double i = loanInterest / 12;                                                           //  i = P / 12, 
            //  K - коэффициент аннуитета
            double coeff = (i * Math.Pow(1 + i, loanTerm)) / (Math.Pow(1 + i, loanTerm) - 1);       //  K = (i * (1 + i) ^ n) / ((1 + i) ^ n - 1)
            //  A - ежемесячный аннуитетный платеж
            double annualPayment = coeff * loanAmount;                                              //  A = K * S
            #endregion

            #region "Вычисление"
            //  Остаток долга, (для первого периода равен сумме займа)
            double remains = loanAmount;
            //  Вычисления для каждого периода
            for (int j = 0; j < loanTerm; j++)
            {
                //  Pn - платеж по %
                double percentDebt = remains * i;                                                   //  Pn = Sn * P / 12, [P / 12 = i]
                //  S - Основной платеж (по телу кредита)
                double bodyDebt = annualPayment - percentDebt;                                      //  s = x - Pn, [x = A]
                //  Добавляем в список (для построения таблицы на клиенте)
                remainders.Add(remains);
                percents.Add(percentDebt);
                bodyDebts.Add(bodyDebt);
                //  Долг уменьшается в конце каждого периода
                remains -= bodyDebt;                                                                //  r(i + 1) = r(i) - s(i)
            }
            #endregion

            ResultData resultData = new ResultData()
            {
                percentDebts = percents,
                bodyDebts = bodyDebts,
                remainderDebts = remainders,
                annualPayment = annualPayment
            };

            return new JsonResult(Ok(resultData));
        }
    }
}
