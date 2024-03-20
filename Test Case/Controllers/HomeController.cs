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
            #region "����������"
            double loanAmount = Convert.ToDouble(amount);                                           //  S - ����� ������� �������
            double loanInterest = Convert.ToDouble(interest) / 100;                                 //  P - ������ (� ���, %)
            int loanTerm = Convert.ToInt32(term);                                                   //  n - ���� ����� (� �������)

            List<double> percents = new List<double>();                                             //  ������� �� %
            List<double> bodyDebts = new List<double>();                                            //  ������� �� ����
            List<double> remainders = new List<double>();                                           //  ���������� ����� �� ��������
            #endregion

            if (loanAmount == 0)
            {
                return null;
            }

            #region "�������� ������"
            //  i - �������� %
            double i = loanInterest / 12;                                                           //  i = P / 12, 
            //  K - ����������� ���������
            double coeff = (i * Math.Pow(1 + i, loanTerm)) / (Math.Pow(1 + i, loanTerm) - 1);       //  K = (i * (1 + i) ^ n) / ((1 + i) ^ n - 1)
            //  A - ����������� ����������� ������
            double annualPayment = coeff * loanAmount;                                              //  A = K * S
            #endregion

            #region "����������"
            //  ������� �����, (��� ������� ������� ����� ����� �����)
            double remains = loanAmount;
            //  ���������� ��� ������� �������
            for (int j = 0; j < loanTerm; j++)
            {
                //  Pn - ������ �� %
                double percentDebt = remains * i;                                                   //  Pn = Sn * P / 12, [P / 12 = i]
                //  S - �������� ������ (�� ���� �������)
                double bodyDebt = annualPayment - percentDebt;                                      //  s = x - Pn, [x = A]
                //  ��������� � ������ (��� ���������� ������� �� �������)
                remainders.Add(remains);
                percents.Add(percentDebt);
                bodyDebts.Add(bodyDebt);
                //  ���� ����������� � ����� ������� �������
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
